using EnterpriseLogIntelligence.Data;
using EnterpriseLogIntelligence.Models;
using MongoDB.Driver;

namespace EnterpriseLogIntelligence.Repositories;

public sealed class MongoLogRepository : ILogRepository
{
    private readonly MongoDbContext _db;
    public MongoLogRepository(MongoDbContext db) => _db = db;

    public async Task InsertManyAsync(IEnumerable<LogEntry> logs, CancellationToken ct = default)
    {
        var list = logs.ToList();
        if (list.Count > 0) await _db.Logs.InsertManyAsync(list, cancellationToken: ct);
    }

    private FilterDefinition<LogEntry> Filter(string? search, string? severity, string? service, string? application, string? environment, DateTime? from, DateTime? to)
    {
        var f = Builders<LogEntry>.Filter.Empty;
        if (!string.IsNullOrWhiteSpace(search))
            f &= Builders<LogEntry>.Filter.Regex(x => x.Message, new MongoDB.Bson.BsonRegularExpression(System.Text.RegularExpressions.Regex.Escape(search), "i"));
        if (!string.IsNullOrWhiteSpace(severity)) f &= Builders<LogEntry>.Filter.Eq(x => x.Severity, severity);
        if (!string.IsNullOrWhiteSpace(service)) f &= Builders<LogEntry>.Filter.Eq(x => x.Service, service);
        if (!string.IsNullOrWhiteSpace(application)) f &= Builders<LogEntry>.Filter.Eq(x => x.Application, application);
        if (!string.IsNullOrWhiteSpace(environment)) f &= Builders<LogEntry>.Filter.Eq(x => x.Environment, environment);
        if (from.HasValue) f &= Builders<LogEntry>.Filter.Gte(x => x.Timestamp, from.Value.ToUniversalTime());
        if (to.HasValue) f &= Builders<LogEntry>.Filter.Lte(x => x.Timestamp, to.Value.ToUniversalTime());
        return f;
    }

    public Task<long> CountAsync(CancellationToken ct = default)
        => _db.Logs.CountDocumentsAsync(FilterDefinition<LogEntry>.Empty, cancellationToken: ct);

    public Task<List<LogEntry>> GetRecentAsync(int take, CancellationToken ct = default)
        => _db.Logs.Find(FilterDefinition<LogEntry>.Empty).SortByDescending(x => x.Timestamp).Limit(take).ToListAsync(ct);

    public Task<List<LogEntry>> SearchAsync(string? search, string? severity, string? service, string? application, string? environment, DateTime? from, DateTime? to, int skip, int take, CancellationToken ct = default)
        => _db.Logs.Find(Filter(search, severity, service, application, environment, from, to)).SortByDescending(x => x.Timestamp).Skip(skip).Limit(take).ToListAsync(ct);

    public Task<long> CountSearchAsync(string? search, string? severity, string? service, string? application, string? environment, DateTime? from, DateTime? to, CancellationToken ct = default)
        => _db.Logs.CountDocumentsAsync(Filter(search, severity, service, application, environment, from, to), cancellationToken: ct);

    public async Task<LogEntry?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        return await _db.Logs.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
    }

    public Task<List<LogEntry>> GetForAnalyticsAsync(DateTime from, DateTime to, CancellationToken ct = default)
        => _db.Logs.Find(Filter(null, null, null, null, null, from, to)).SortByDescending(x => x.Timestamp).ToListAsync(ct);
}
