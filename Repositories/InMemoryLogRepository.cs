using System.Collections.Concurrent;
using EnterpriseLogIntelligence.Models;

namespace EnterpriseLogIntelligence.Repositories;

public sealed class InMemoryLogRepository : ILogRepository
{
    private readonly ConcurrentDictionary<string, LogEntry> _logs = new();

    public Task InsertManyAsync(IEnumerable<LogEntry> logs, CancellationToken ct = default)
    {
        foreach (var log in logs)
        {
            ct.ThrowIfCancellationRequested();
            _logs[log.Id] = log;
        }
        return Task.CompletedTask;
    }

    public Task<long> CountAsync(CancellationToken ct = default) => Task.FromResult((long)_logs.Count);

    public Task<List<LogEntry>> GetRecentAsync(int take, CancellationToken ct = default)
        => Task.FromResult(_logs.Values.OrderByDescending(x => x.Timestamp).Take(take).ToList());

    public Task<List<LogEntry>> SearchAsync(string? search, string? severity, string? service, string? application, string? environment, DateTime? from, DateTime? to, int skip, int take, CancellationToken ct = default)
    {
        IEnumerable<LogEntry> q = _logs.Values;
        if (!string.IsNullOrWhiteSpace(search)) q = q.Where(x => x.Message.Contains(search, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(severity)) q = q.Where(x => x.Severity.Equals(severity, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(service)) q = q.Where(x => x.Service.Equals(service, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(application)) q = q.Where(x => x.Application.Equals(application, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(environment)) q = q.Where(x => x.Environment.Equals(environment, StringComparison.OrdinalIgnoreCase));
        if (from.HasValue) q = q.Where(x => x.Timestamp >= from.Value.ToUniversalTime());
        if (to.HasValue) q = q.Where(x => x.Timestamp <= to.Value.ToUniversalTime());
        return Task.FromResult(q.OrderByDescending(x => x.Timestamp).Skip(skip).Take(take).ToList());
    }

    public async Task<long> CountSearchAsync(string? search, string? severity, string? service, string? application, string? environment, DateTime? from, DateTime? to, CancellationToken ct = default)
        => (await SearchAsync(search, severity, service, application, environment, from, to, 0, int.MaxValue, ct)).LongCount();

    public Task<LogEntry?> GetByIdAsync(string id, CancellationToken ct = default)
        => Task.FromResult(_logs.TryGetValue(id, out var log) ? log : null);

    public Task<List<LogEntry>> GetForAnalyticsAsync(DateTime from, DateTime to, CancellationToken ct = default)
        => Task.FromResult(_logs.Values.Where(x => x.Timestamp >= from && x.Timestamp <= to).OrderByDescending(x => x.Timestamp).ToList());
}
