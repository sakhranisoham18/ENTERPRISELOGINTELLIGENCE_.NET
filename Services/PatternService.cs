using System.Text.RegularExpressions;
using EnterpriseLogIntelligence.Models;
using EnterpriseLogIntelligence.Repositories;
using EnterpriseLogIntelligence.ViewModels;

namespace EnterpriseLogIntelligence.Services;

public interface IPatternService
{
    Task<List<PatternViewModel>> GetPatternsAsync(CancellationToken ct = default);
}

public sealed class PatternService : IPatternService
{
    private readonly ILogRepository _logs;
    private readonly IPatternRepository _patterns;

    public PatternService(ILogRepository logs, IPatternRepository patterns)
    {
        _logs = logs; _patterns = patterns;
    }

    public async Task<List<PatternViewModel>> GetPatternsAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var logs = await _logs.GetForAnalyticsAsync(now.AddDays(-7), now, ct);
        var groups = logs.GroupBy(x => new { Pattern = Normalize(x.Message), x.Service, x.Severity })
            .OrderByDescending(g => g.Count())
            .Take(50)
            .ToList();

        foreach (var group in groups)
        {
            var events = group.ToList();
            await _patterns.UpsertAsync(new LogPattern
            {
                Name = group.Key.Pattern,
                Service = group.Key.Service,
                Severity = group.Key.Severity,
                Occurrences = events.Count,
                FirstSeen = events.Min(x => x.Timestamp),
                LastSeen = events.Max(x => x.Timestamp)
            }, ct);
        }

        return groups.Take(20).Select(g => new PatternViewModel
        {
            Name = g.Key.Pattern,
            Occurrences = g.LongCount(),
            Severity = g.Key.Severity,
            Service = g.Key.Service
        }).ToList();
    }

    private static string Normalize(string message)
    {
        var text = Regex.Replace(message, @"\b[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[1-5][0-9a-fA-F]{3}-[89abAB][0-9a-fA-F]{3}-[0-9a-fA-F]{12}\b", "{GUID}");
        text = Regex.Replace(text, @"\b(?:\d{1,3}\.){3}\d{1,3}\b", "{IP}");
        text = Regex.Replace(text, @"\b\d+\b", "{N}");
        return text.Trim();
    }
}
