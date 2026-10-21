using System.Collections.Concurrent;
using EnterpriseLogIntelligence.Data;
using EnterpriseLogIntelligence.Models;
using MongoDB.Driver;

namespace EnterpriseLogIntelligence.Repositories;

public interface IAuditLogRepository
{
    Task InsertAsync(AuditLog auditLog, CancellationToken ct = default);
    Task<List<AuditLog>> GetRecentAsync(int take, CancellationToken ct = default);
}

public sealed class MongoAuditLogRepository : IAuditLogRepository
{
    private readonly MongoDbContext _db;
    public MongoAuditLogRepository(MongoDbContext db) => _db = db;
    public Task InsertAsync(AuditLog auditLog, CancellationToken ct = default) => _db.AuditLogs.InsertOneAsync(auditLog, cancellationToken: ct);
    public Task<List<AuditLog>> GetRecentAsync(int take, CancellationToken ct = default) => _db.AuditLogs.Find(Builders<AuditLog>.Filter.Empty).SortByDescending(x => x.Timestamp).Limit(take).ToListAsync(ct);
}

public sealed class InMemoryAuditLogRepository : IAuditLogRepository
{
    private readonly ConcurrentQueue<AuditLog> _logs = new();
    public Task InsertAsync(AuditLog auditLog, CancellationToken ct = default) { _logs.Enqueue(auditLog); while (_logs.Count > 5000 && _logs.TryDequeue(out _)) { } return Task.CompletedTask; }
    public Task<List<AuditLog>> GetRecentAsync(int take, CancellationToken ct = default) => Task.FromResult(_logs.OrderByDescending(x => x.Timestamp).Take(take).ToList());
}
