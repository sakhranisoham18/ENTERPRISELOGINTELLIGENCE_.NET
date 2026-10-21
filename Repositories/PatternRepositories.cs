using System.Collections.Concurrent;
using EnterpriseLogIntelligence.Data;
using EnterpriseLogIntelligence.Models;
using MongoDB.Driver;

namespace EnterpriseLogIntelligence.Repositories;

public interface IPatternRepository
{
    Task UpsertAsync(LogPattern pattern, CancellationToken ct = default);
    Task<List<LogPattern>> GetRecentAsync(int take, CancellationToken ct = default);
}

public sealed class MongoPatternRepository : IPatternRepository
{
    private readonly MongoDbContext _db;
    public MongoPatternRepository(MongoDbContext db) => _db = db;
    public Task UpsertAsync(LogPattern pattern, CancellationToken ct = default)
    {
        var filter = Builders<LogPattern>.Filter.Eq(x => x.Name, pattern.Name)
            & Builders<LogPattern>.Filter.Eq(x => x.Service, pattern.Service)
            & Builders<LogPattern>.Filter.Eq(x => x.Severity, pattern.Severity);
        var update = Builders<LogPattern>.Update
            .Set(x => x.Occurrences, pattern.Occurrences)
            .Set(x => x.FirstSeen, pattern.FirstSeen)
            .Set(x => x.LastSeen, pattern.LastSeen)
            .SetOnInsert(x => x.Name, pattern.Name)
            .SetOnInsert(x => x.Service, pattern.Service)
            .SetOnInsert(x => x.Severity, pattern.Severity);
        return _db.Patterns.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
    }
    public Task<List<LogPattern>> GetRecentAsync(int take, CancellationToken ct = default)
        => _db.Patterns.Find(Builders<LogPattern>.Filter.Empty).SortByDescending(x => x.LastSeen).Limit(take).ToListAsync(ct);
}

public sealed class InMemoryPatternRepository : IPatternRepository
{
    private readonly ConcurrentDictionary<string, LogPattern> _patterns = new();
    private static string Key(LogPattern p) => $"{p.Service}|{p.Severity}|{p.Name}";
    public Task UpsertAsync(LogPattern pattern, CancellationToken ct = default) { _patterns[Key(pattern)] = pattern; return Task.CompletedTask; }
    public Task<List<LogPattern>> GetRecentAsync(int take, CancellationToken ct = default) => Task.FromResult(_patterns.Values.OrderByDescending(x => x.LastSeen).Take(take).ToList());
}
