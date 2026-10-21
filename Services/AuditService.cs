using EnterpriseLogIntelligence.Models;
using EnterpriseLogIntelligence.Repositories;

namespace EnterpriseLogIntelligence.Services;

public interface IAuditService
{
    Task RecordAsync(string username, string action, string details, CancellationToken ct = default);
    Task<List<AuditLog>> GetRecentAsync(int take = 100, CancellationToken ct = default);
}

public sealed class AuditService : IAuditService
{
    private readonly IAuditLogRepository _repo;
    public AuditService(IAuditLogRepository repo) => _repo = repo;
    public Task RecordAsync(string username, string action, string details, CancellationToken ct = default)
        => _repo.InsertAsync(new AuditLog { Username = string.IsNullOrWhiteSpace(username) ? "system" : username, Action = action, Details = details }, ct);
    public Task<List<AuditLog>> GetRecentAsync(int take = 100, CancellationToken ct = default) => _repo.GetRecentAsync(take, ct);
}
