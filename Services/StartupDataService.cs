using EnterpriseLogIntelligence.Models;
using EnterpriseLogIntelligence.Repositories;

namespace EnterpriseLogIntelligence.Services;

public sealed class StartupDataService
{
    private readonly ILogRepository _logs;
    private readonly IAuthService _auth;
    private readonly Data.MongoDbContext _mongo;
    private readonly IConfiguration _config;
    private readonly ILogger<StartupDataService> _logger;

    public StartupDataService(ILogRepository logs, IAuthService auth, Data.MongoDbContext mongo, IConfiguration config, ILogger<StartupDataService> logger)
    {
        _logs = logs; _auth = auth; _mongo = mongo; _config = config; _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        var provider = (_config["Storage:Provider"] ?? "InMemory").Trim();
        if (provider.Equals("MongoDB", StringComparison.OrdinalIgnoreCase))
        {
            if (!await _mongo.PingAsync(ct))
            {
                _logger.LogError("MongoDB is configured but not reachable. Check MongoDb:ConnectionString, Atlas IP access, database user credentials, and network connectivity.");
                return;
            }
            await _mongo.EnsureIndexesAsync(ct);
            await _auth.EnsureAdminAsync(ct);
            return;
        }

        if (await _logs.CountAsync(ct) > 0) return;
        var seed = DemoSeed.Create();
        await _logs.InsertManyAsync(seed, ct);
        await _auth.EnsureAdminAsync(ct);
    }
}

internal static class DemoSeed
{
    public static List<LogEntry> Create()
    {
        var now = DateTime.UtcNow;
        var services = new[] { "Auth Service", "Payment Service", "Order Service", "API Gateway", "Database Service", "Notification Service" };
        var apps = new[] { "WebPortal", "MobileAPI", "AdminConsole" };
        var messages = new Dictionary<string, string[]>
        {
            ["DEBUG"] = new[] { "Cache lookup started", "Request validation completed" },
            ["INFO"] = new[] { "User session created", "API request completed", "Order retrieved successfully", "Background job completed" },
            ["WARNING"] = new[] { "Slow database response detected", "Retry scheduled for downstream request", "Cache miss rate increased" },
            ["ERROR"] = new[] { "Database connection failed", "Payment gateway timeout", "Unhandled API exception", "Order processing failed" },
            ["CRITICAL"] = new[] { "Service health check failed", "Database unavailable", "Payment processing outage" },
            ["SECURITY"] = new[] { "Multiple failed login attempts", "Suspicious request signature detected" }
        };
        var rng = new Random(42); var list = new List<LogEntry>();
        for (var i = 0; i < 900; i++)
        {
            var roll = rng.Next(100);
            var sev = roll < 8 ? "DEBUG" : roll < 58 ? "INFO" : roll < 80 ? "WARNING" : roll < 96 ? "ERROR" : roll < 99 ? "CRITICAL" : "SECURITY";
            list.Add(new LogEntry
            {
                Timestamp = now.AddMinutes(-rng.Next(0, 24 * 60)), Severity = sev, Application = apps[rng.Next(apps.Length)],
                Service = services[rng.Next(services.Length)], Source = "DemoSeed", Message = messages[sev][rng.Next(messages[sev].Length)],
                IpAddress = $"10.0.{rng.Next(1, 20)}.{rng.Next(2, 250)}", StatusCode = sev is "INFO" or "DEBUG" ? 200 : sev == "WARNING" ? 429 : 500,
                Environment = "Demo", RequestId = Guid.NewGuid().ToString("N")[..12]
            });
        }
        return list;
    }
}
