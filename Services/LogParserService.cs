using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using EnterpriseLogIntelligence.Models;

namespace EnterpriseLogIntelligence.Services;

public interface ILogParserService
{
    Task<(List<LogEntry> Logs, List<string> Warnings)> ParseAsync(Stream stream, string fileName, CancellationToken ct = default);
}

public sealed class LogParserService : ILogParserService
{
    private static readonly Regex LineRegex = new(
        @"^\[?(?<ts>\d{4}-\d{2}-\d{2}[ T]\d{2}:\d{2}:\d{2}(?:\.\d+)?)\]?\s+(?<sev>DEBUG|INFO|WARN(?:ING)?|ERROR|CRITICAL|SECURITY)\s+(?<message>.*)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex IpRegex = new(@"\b(?:\d{1,3}\.){3}\d{1,3}\b", RegexOptions.Compiled);
    private static readonly Regex GuidRegex = new(@"\b[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[1-5][0-9a-fA-F]{3}-[89abAB][0-9a-fA-F]{3}-[0-9a-fA-F]{12}\b", RegexOptions.Compiled);

    public async Task<(List<LogEntry> Logs, List<string> Warnings)> ParseAsync(Stream stream, string fileName, CancellationToken ct = default)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var text = await reader.ReadToEndAsync(ct);
        if (string.IsNullOrWhiteSpace(text)) return (new List<LogEntry>(), new List<string> { "The uploaded file was empty." });

        var trimmed = text.TrimStart();
        if (trimmed.StartsWith("[")) return ParseJsonArray(trimmed, fileName);
        if (trimmed.StartsWith("{")) return ParseJsonLinesOrObject(text, fileName);

        var lines = text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        if (LooksLikeCsv(lines.FirstOrDefault())) return ParseCsv(lines, fileName);
        return ParseText(lines, fileName);
    }

    private static (List<LogEntry>, List<string>) ParseJsonArray(string text, string fileName)
    {
        var logs = new List<LogEntry>();
        var warnings = new List<string>();
        try
        {
            using var doc = JsonDocument.Parse(text);
            if (doc.RootElement.ValueKind != JsonValueKind.Array) return ParseJsonLinesOrObject(text, fileName);
            foreach (var item in doc.RootElement.EnumerateArray())
            {
                if (TryMapJson(item, fileName, out var log)) logs.Add(log!); else warnings.Add("Skipped one JSON record that could not be mapped to a log entry.");
            }
        }
        catch (JsonException ex) { warnings.Add($"JSON parsing failed: {ex.Message}"); }
        return (logs, warnings);
    }

    private static (List<LogEntry>, List<string>) ParseJsonLinesOrObject(string text, string fileName)
    {
        var logs = new List<LogEntry>();
        var warnings = new List<string>();
        var lines = text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            try
            {
                using var doc = JsonDocument.Parse(line);
                if (doc.RootElement.ValueKind == JsonValueKind.Object && TryMapJson(doc.RootElement, fileName, out var log)) logs.Add(log!);
                else warnings.Add("Skipped one JSON line that is not a valid log object.");
            }
            catch (JsonException) { warnings.Add("Skipped one invalid JSON line."); }
        }
        return (logs, warnings);
    }

    private static bool TryMapJson(JsonElement item, string fileName, out LogEntry? log)
    {
        log = new LogEntry
        {
            Source = fileName,
            Application = "UploadedFile",
            Service = "Imported",
            Environment = "Imported"
        };

        log.Timestamp = ReadDate(item, "timestamp", "time", "@timestamp") ?? DateTime.UtcNow;
        log.Severity = NormalizeSeverity(ReadString(item, "severity", "level", "logLevel") ?? "INFO");
        log.Application = ReadString(item, "application", "app") ?? log.Application;
        log.Service = ReadString(item, "service", "component") ?? log.Service;
        log.Source = ReadString(item, "source", "logger") ?? log.Source;
        log.Message = ReadString(item, "message", "msg") ?? string.Empty;
        log.IpAddress = ReadString(item, "ipAddress", "ip", "clientIp") ?? log.IpAddress;
        log.RequestId = ReadString(item, "requestId", "traceId", "correlationId") ?? log.RequestId;
        log.Environment = ReadString(item, "environment", "env") ?? log.Environment;
        var status = ReadString(item, "statusCode", "status");
        if (int.TryParse(status, out var statusCode)) log.StatusCode = statusCode;

        if (string.IsNullOrWhiteSpace(log.Message)) return false;
        return true;
    }

    private static (List<LogEntry>, List<string>) ParseText(List<string> lines, string fileName)
    {
        var logs = new List<LogEntry>();
        var warnings = new List<string>();
        foreach (var line in lines)
        {
            var match = LineRegex.Match(line.Trim());
            if (match.Success)
            {
                var ts = DateTime.TryParse(match.Groups["ts"].Value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsed) ? parsed.ToUniversalTime() : DateTime.UtcNow;
                var sev = NormalizeSeverity(match.Groups["sev"].Value);
                logs.Add(CreateLog(fileName, match.Groups["message"].Value, sev, ts));
                continue;
            }
            logs.Add(CreateLog(fileName, line.Trim(), InferSeverity(line), DateTime.UtcNow));
        }
        return (logs, warnings);
    }

    private static (List<LogEntry>, List<string>) ParseCsv(List<string> lines, string fileName)
    {
        var logs = new List<LogEntry>();
        var warnings = new List<string>();
        var headers = SplitCsv(lines[0]).Select(x => x.Trim().ToLowerInvariant()).ToList();
        for (var i = 1; i < lines.Count; i++)
        {
            var cells = SplitCsv(lines[i]);
            if (cells.Count == 0) continue;
            string Get(params string[] names)
            {
                foreach (var name in names)
                {
                    var idx = headers.IndexOf(name.ToLowerInvariant());
                    if (idx >= 0 && idx < cells.Count) return cells[idx];
                }
                return string.Empty;
            }
            var message = Get("message", "msg", "log");
            if (string.IsNullOrWhiteSpace(message)) { warnings.Add($"Skipped CSV row {i + 1}: no message column."); continue; }
            var timestamp = DateTime.TryParse(Get("timestamp", "time", "date"), out var ts) ? ts.ToUniversalTime() : DateTime.UtcNow;
            var severity = NormalizeSeverity(Get("severity", "level"));
            var log = CreateLog(fileName, message, severity, timestamp);
            log.Application = Get("application", "app") is { Length: > 0 } app ? app : log.Application;
            log.Service = Get("service", "component") is { Length: > 0 } service ? service : log.Service;
            log.Environment = Get("environment", "env") is { Length: > 0 } env ? env : log.Environment;
            log.IpAddress = Get("ip", "ipaddress", "clientip") is { Length: > 0 } ip ? ip : log.IpAddress;
            log.RequestId = Get("requestid", "traceid", "correlationid") is { Length: > 0 } req ? req : log.RequestId;
            if (int.TryParse(Get("statuscode", "status"), out var code)) log.StatusCode = code;
            logs.Add(log);
        }
        return (logs, warnings);
    }

    private static bool LooksLikeCsv(string? firstLine)
        => !string.IsNullOrWhiteSpace(firstLine) && firstLine.Contains(',') && (firstLine.Contains("message", StringComparison.OrdinalIgnoreCase) || firstLine.Contains("severity", StringComparison.OrdinalIgnoreCase) || firstLine.Contains("timestamp", StringComparison.OrdinalIgnoreCase));

    private static List<string> SplitCsv(string line)
    {
        var result = new List<string>(); var sb = new StringBuilder(); var quoted = false;
        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"')
            {
                if (quoted && i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                else quoted = !quoted;
            }
            else if (c == ',' && !quoted) { result.Add(sb.ToString()); sb.Clear(); }
            else sb.Append(c);
        }
        result.Add(sb.ToString());
        return result;
    }

    private static LogEntry CreateLog(string fileName, string message, string severity, DateTime timestamp)
        => new()
        {
            Timestamp = timestamp,
            Severity = severity,
            Application = "UploadedFile",
            Service = "Imported",
            Source = fileName,
            Message = message,
            IpAddress = ExtractIp(message) ?? "127.0.0.1",
            StatusCode = severity == "INFO" ? 200 : severity == "WARNING" ? 429 : severity == "ERROR" ? 500 : 503,
            Environment = "Imported"
        };

    private static string NormalizeSeverity(string value)
    {
        var upper = value.Trim().ToUpperInvariant();
        return upper switch { "WARN" => "WARNING", "FATAL" => "CRITICAL", "TRACE" => "DEBUG", _ when new[] { "DEBUG", "INFO", "WARNING", "ERROR", "CRITICAL", "SECURITY" }.Contains(upper) => upper, _ => InferSeverity(upper) };
    }

    private static string InferSeverity(string value)
        => value.Contains("CRITICAL", StringComparison.OrdinalIgnoreCase) || value.Contains("FATAL", StringComparison.OrdinalIgnoreCase) ? "CRITICAL"
         : value.Contains("SECURITY", StringComparison.OrdinalIgnoreCase) || value.Contains("AUTH", StringComparison.OrdinalIgnoreCase) ? "SECURITY"
         : value.Contains("ERROR", StringComparison.OrdinalIgnoreCase) || value.Contains("EXCEPTION", StringComparison.OrdinalIgnoreCase) ? "ERROR"
         : value.Contains("WARN", StringComparison.OrdinalIgnoreCase) ? "WARNING" : value.Contains("DEBUG", StringComparison.OrdinalIgnoreCase) ? "DEBUG" : "INFO";

    private static string? ExtractIp(string value) => IpRegex.Match(value).Success ? IpRegex.Match(value).Value : null;

    private static string? ReadString(JsonElement item, params string[] names)
    {
        foreach (var property in item.EnumerateObject())
            if (names.Any(x => string.Equals(x, property.Name, StringComparison.OrdinalIgnoreCase)))
                return property.Value.ValueKind == JsonValueKind.String ? property.Value.GetString() : property.Value.ToString();
        return null;
    }

    private static DateTime? ReadDate(JsonElement item, params string[] names)
    {
        var value = ReadString(item, names);
        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed) ? parsed : null;
    }
}
