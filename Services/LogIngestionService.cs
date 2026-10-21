using System.Text.Json;
using EnterpriseLogIntelligence.Hubs;
using EnterpriseLogIntelligence.Models;
using EnterpriseLogIntelligence.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;

namespace EnterpriseLogIntelligence.Services;

public interface ILogIngestionService
{
    Task<int> IngestAsync(IEnumerable<LogEntry> logs, CancellationToken ct = default);
}

public sealed class LogIngestionService : ILogIngestionService
{
    private readonly ILogRepository _logs;
    private readonly IAlertEvaluationService _alerts;
    private readonly IAuditService _audit;
    private readonly IHubContext<DashboardHub> _hub;
    private readonly IConfiguration _config;

    public LogIngestionService(ILogRepository logs, IAlertEvaluationService alerts, IAuditService audit, IHubContext<DashboardHub> hub, IConfiguration config)
    {
        _logs = logs; _alerts = alerts; _audit = audit; _hub = hub; _config = config;
    }

    public async Task<int> IngestAsync(IEnumerable<LogEntry> logs, CancellationToken ct = default)
    {
        var batch = logs.Where(x => !string.IsNullOrWhiteSpace(x.Message)).Take(_config.GetValue<int?>("Application:MaxIngestionBatch") ?? 10000).ToList();
        foreach (var log in batch) Normalize(log);
        if (batch.Count == 0) return 0;

        await _logs.InsertManyAsync(batch, ct);
        await _alerts.EvaluateAsync(ct);
        await _audit.RecordAsync("system", "Logs.Ingested", $"Accepted {batch.Count:N0} log events.", ct);
        await _hub.Clients.All.SendAsync("logsIngested", new { count = batch.Count, timestamp = DateTime.UtcNow }, ct);
        return batch.Count;
    }

    private static void Normalize(LogEntry log)
    {
        log.Id = string.IsNullOrWhiteSpace(log.Id) ? MongoDB.Bson.ObjectId.GenerateNewId().ToString() : log.Id;
        log.Severity = NormalizeSeverity(log.Severity);
        log.Application = string.IsNullOrWhiteSpace(log.Application) ? "UnknownApp" : log.Application.Trim();
        log.Service = string.IsNullOrWhiteSpace(log.Service) ? "UnknownService" : log.Service.Trim();
        log.Source = string.IsNullOrWhiteSpace(log.Source) ? "ApplicationServer" : log.Source.Trim();
        log.Message = log.Message.Trim();
        log.Environment = string.IsNullOrWhiteSpace(log.Environment) ? "Development" : log.Environment.Trim();
        if (log.Timestamp.Kind == DateTimeKind.Unspecified) log.Timestamp = DateTime.SpecifyKind(log.Timestamp, DateTimeKind.Utc);
        log.Timestamp = log.Timestamp.ToUniversalTime();
        log.RequestId = string.IsNullOrWhiteSpace(log.RequestId) ? Guid.NewGuid().ToString("N")[..12] : log.RequestId.Trim();
        log.IpAddress = string.IsNullOrWhiteSpace(log.IpAddress) ? "127.0.0.1" : log.IpAddress.Trim();
    }

    private static string NormalizeSeverity(string value)
    {
        var s = (value ?? string.Empty).Trim().ToUpperInvariant();
        return s switch { "WARN" => "WARNING", "FATAL" => "CRITICAL", _ when new[] { "DEBUG", "INFO", "WARNING", "ERROR", "CRITICAL", "SECURITY" }.Contains(s) => s, _ => "INFO" };
    }
}
