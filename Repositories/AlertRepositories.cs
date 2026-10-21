using EnterpriseLogIntelligence.Data;
using EnterpriseLogIntelligence.Models;
using MongoDB.Driver;
using System.Collections.Concurrent;

namespace EnterpriseLogIntelligence.Repositories;

public interface IAlertRepository
{
    Task<List<Alert>> GetRulesAsync(CancellationToken ct = default);
    Task InsertRuleAsync(Alert alert, CancellationToken ct = default);
    Task UpdateRuleAsync(Alert alert, CancellationToken ct = default);
    Task DeleteRuleAsync(string id, CancellationToken ct = default);
    Task InsertEventAsync(AlertEvent alertEvent, CancellationToken ct = default);
    Task<List<AlertEvent>> GetEventsAsync(int take, CancellationToken ct = default);
}

public sealed class MongoAlertRepository : IAlertRepository
{
    private readonly MongoDbContext _db;
    public MongoAlertRepository(MongoDbContext db) => _db = db;
    public Task<List<Alert>> GetRulesAsync(CancellationToken ct = default) => _db.Alerts.Find(Builders<Alert>.Filter.Empty).SortByDescending(x => x.CreatedAt).ToListAsync(ct);
    public Task InsertRuleAsync(Alert alert, CancellationToken ct = default) => _db.Alerts.InsertOneAsync(alert, cancellationToken: ct);
    public Task UpdateRuleAsync(Alert alert, CancellationToken ct = default) => _db.Alerts.ReplaceOneAsync(x => x.Id == alert.Id, alert, cancellationToken: ct);
    public Task DeleteRuleAsync(string id, CancellationToken ct = default) => _db.Alerts.DeleteOneAsync(x => x.Id == id, ct);
    public Task InsertEventAsync(AlertEvent alertEvent, CancellationToken ct = default) => _db.AlertEvents.InsertOneAsync(alertEvent, cancellationToken: ct);
    public Task<List<AlertEvent>> GetEventsAsync(int take, CancellationToken ct = default) => _db.AlertEvents.Find(Builders<AlertEvent>.Filter.Empty).SortByDescending(x => x.TriggeredAt).Limit(take).ToListAsync(ct);
}

public sealed class InMemoryAlertRepository : IAlertRepository
{
    private readonly ConcurrentDictionary<string, Alert> _rules = new();
    private readonly ConcurrentDictionary<string, AlertEvent> _events = new();
    public Task<List<Alert>> GetRulesAsync(CancellationToken ct = default) => Task.FromResult(_rules.Values.OrderByDescending(x => x.CreatedAt).ToList());
    public Task InsertRuleAsync(Alert alert, CancellationToken ct = default) { _rules[alert.Id] = alert; return Task.CompletedTask; }
    public Task UpdateRuleAsync(Alert alert, CancellationToken ct = default) { _rules[alert.Id] = alert; return Task.CompletedTask; }
    public Task DeleteRuleAsync(string id, CancellationToken ct = default) { _rules.TryRemove(id, out _); return Task.CompletedTask; }
    public Task InsertEventAsync(AlertEvent alertEvent, CancellationToken ct = default) { _events[alertEvent.Id] = alertEvent; return Task.CompletedTask; }
    public Task<List<AlertEvent>> GetEventsAsync(int take, CancellationToken ct = default) => Task.FromResult(_events.Values.OrderByDescending(x => x.TriggeredAt).Take(take).ToList());
}
