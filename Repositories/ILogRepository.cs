using EnterpriseLogIntelligence.Models;

namespace EnterpriseLogIntelligence.Repositories;

public interface ILogRepository
{
    Task InsertManyAsync(IEnumerable<LogEntry> logs, CancellationToken ct = default);
    Task<long> CountAsync(CancellationToken ct = default);
    Task<List<LogEntry>> GetRecentAsync(int take, CancellationToken ct = default);
    Task<List<LogEntry>> SearchAsync(string? search, string? severity, string? service, string? application, string? environment, DateTime? from, DateTime? to, int skip, int take, CancellationToken ct = default);
    Task<long> CountSearchAsync(string? search, string? severity, string? service, string? application, string? environment, DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<LogEntry?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<List<LogEntry>> GetForAnalyticsAsync(DateTime from, DateTime to, CancellationToken ct = default);
}
