using EnterpriseLogIntelligence.Models;

namespace EnterpriseLogIntelligence.Services;

public interface ILogGeneratorService { Task<int> GenerateAsync(int count, CancellationToken ct = default); }

public sealed class LogGeneratorService : ILogGeneratorService
{
    private readonly ILogIngestionService _ingestion; private readonly Random _rng = new();
    private readonly string[] _services = { "Auth Service", "Payment Service", "Order Service", "API Gateway", "Database Service", "Notification Service" };
    private readonly string[] _apps = { "WebPortal", "MobileAPI", "AdminConsole" };
    private readonly Dictionary<string, string[]> _messages = new()
    {
        ["DEBUG"] = new[] { "Cache lookup started", "Request validation completed" },
        ["INFO"] = new[] { "User session created", "API request completed", "Order retrieved successfully", "Background job completed" },
        ["WARNING"] = new[] { "Slow database response detected", "Retry scheduled for downstream request", "Cache miss rate increased" },
        ["ERROR"] = new[] { "Database connection failed", "Payment gateway timeout", "Unhandled API exception", "Order processing failed" },
        ["CRITICAL"] = new[] { "Service health check failed", "Database unavailable", "Payment processing outage" },
        ["SECURITY"] = new[] { "Multiple failed login attempts", "Suspicious request signature detected" }
    };

    public LogGeneratorService(ILogIngestionService ingestion) => _ingestion = ingestion;

    public async Task<int> GenerateAsync(int count, CancellationToken ct = default)
    {
        count = Math.Clamp(count, 1, 2000);
        var logs = new List<LogEntry>(count);
        for (var i = 0; i < count; i++)
        {
            var roll = _rng.Next(100);
            var sev = roll < 50 ? "INFO" : roll < 68 ? "DEBUG" : roll < 84 ? "WARNING" : roll < 96 ? "ERROR" : roll < 99 ? "CRITICAL" : "SECURITY";
            logs.Add(new LogEntry
            {
                Timestamp = DateTime.UtcNow.AddSeconds(-_rng.Next(0, 3600)),
                Severity = sev,
                Application = _apps[_rng.Next(_apps.Length)],
                Service = _services[_rng.Next(_services.Length)],
                Source = "GeneratedSource",
                Message = _messages[sev][_rng.Next(_messages[sev].Length)],
                IpAddress = $"10.20.{_rng.Next(1, 20)}.{_rng.Next(2, 250)}",
                StatusCode = sev is "INFO" or "DEBUG" ? 200 : sev == "WARNING" ? 429 : sev == "ERROR" ? 500 : 503,
                Environment = "Demo",
                RequestId = Guid.NewGuid().ToString("N")[..12]
            });
        }
        return await _ingestion.IngestAsync(logs, ct);
    }
}
