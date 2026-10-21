using EnterpriseLogIntelligence.Repositories;
using EnterpriseLogIntelligence.ViewModels;

namespace EnterpriseLogIntelligence.Services;

public interface IAnalyticsService { Task<DashboardViewModel> BuildDashboardAsync(CancellationToken ct = default); }

public sealed class AnalyticsService : IAnalyticsService
{
    private readonly ILogRepository _logs;
    public AnalyticsService(ILogRepository logs) => _logs = logs;

    public async Task<DashboardViewModel> BuildDashboardAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow; var from = now.AddHours(-24); var logs = await _logs.GetForAnalyticsAsync(from, now, ct);
        var total = logs.Count; var errors = logs.LongCount(x => x.Severity == "ERROR"); var warnings = logs.LongCount(x => x.Severity == "WARNING"); var critical = logs.LongCount(x => x.Severity == "CRITICAL");
        var volume = Enumerable.Range(0, 24).Select(h => { var start = from.AddHours(h); return new ChartPoint { Label = start.ToLocalTime().ToString("HH:mm"), Value = logs.LongCount(x => x.Timestamp >= start && x.Timestamp < start.AddHours(1)) }; }).ToList();
        var severity = logs.GroupBy(x => x.Severity).Select(g => new ChartPoint { Label = g.Key, Value = g.Count() }).OrderByDescending(x => x.Value).ToList();
        var services = logs.GroupBy(x => x.Service).Select(g => new ChartPoint { Label = g.Key, Value = g.Count() }).OrderByDescending(x => x.Value).Take(6).ToList();
        var patterns = logs.GroupBy(x => new { x.Service, x.Message, x.Severity }).OrderByDescending(g => g.Count()).Take(5).Select(g => new PatternViewModel { Name = g.Key.Message, Occurrences = g.Count(), Severity = g.Key.Severity, Service = g.Key.Service }).ToList();
        var recent = logs.Where(x => x.Severity is "CRITICAL" or "SECURITY").OrderByDescending(x => x.Timestamp).Take(8).Select(x => new LogRowViewModel { Id = x.Id, Timestamp = x.Timestamp, Severity = x.Severity, Service = x.Service, Message = x.Message }).ToList();
        return new DashboardViewModel { TotalLogs = total, Errors = errors, Warnings = warnings, Critical = critical, ErrorRate = total == 0 ? 0 : (double)errors / total * 100, IngestionRate = total / 24d, Volume = volume, Severity = severity, Services = services, Patterns = patterns, RecentCritical = recent };
    }
}
