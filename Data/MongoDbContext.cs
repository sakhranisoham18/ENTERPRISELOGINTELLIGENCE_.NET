using EnterpriseLogIntelligence.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EnterpriseLogIntelligence.Data;

public sealed class MongoDbContext
{
    public IMongoClient Client { get; }
    public IMongoDatabase Database { get; }
    public IMongoCollection<LogEntry> Logs => Database.GetCollection<LogEntry>("logs");
    public IMongoCollection<LogPattern> Patterns => Database.GetCollection<LogPattern>("logPatterns");
    public IMongoCollection<Alert> Alerts => Database.GetCollection<Alert>("alerts");
    public IMongoCollection<AlertEvent> AlertEvents => Database.GetCollection<AlertEvent>("alertEvents");
    public IMongoCollection<User> Users => Database.GetCollection<User>("users");
    public IMongoCollection<AuditLog> AuditLogs => Database.GetCollection<AuditLog>("auditLogs");

    public MongoDbContext(IOptions<MongoDbSettings> options)
    {
        var settings = options.Value;
        var connectionString = string.IsNullOrWhiteSpace(settings.ConnectionString)
            ? "mongodb://127.0.0.1:27017"
            : settings.ConnectionString;

        Client = new MongoClient(connectionString);
        Database = Client.GetDatabase(settings.DatabaseName);
    }

    public async Task EnsureIndexesAsync(CancellationToken ct = default)
    {
        await Logs.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<LogEntry>(Builders<LogEntry>.IndexKeys.Descending(x => x.Timestamp)),
            new CreateIndexModel<LogEntry>(Builders<LogEntry>.IndexKeys.Ascending(x => x.Severity)),
            new CreateIndexModel<LogEntry>(Builders<LogEntry>.IndexKeys.Ascending(x => x.Service)),
            new CreateIndexModel<LogEntry>(Builders<LogEntry>.IndexKeys.Ascending(x => x.Application)),
            new CreateIndexModel<LogEntry>(Builders<LogEntry>.IndexKeys.Ascending(x => x.Environment)),
            new CreateIndexModel<LogEntry>(Builders<LogEntry>.IndexKeys.Ascending(x => x.RequestId))
        }, ct);

        await Patterns.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<LogPattern>(Builders<LogPattern>.IndexKeys
                .Ascending(x => x.Name).Ascending(x => x.Service).Ascending(x => x.Severity))
        }, ct);

        await Alerts.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<Alert>(Builders<Alert>.IndexKeys.Ascending(x => x.IsEnabled))
        }, ct);

        await AlertEvents.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<AlertEvent>(Builders<AlertEvent>.IndexKeys.Descending(x => x.TriggeredAt)),
            new CreateIndexModel<AlertEvent>(Builders<AlertEvent>.IndexKeys.Ascending(x => x.RuleId))
        }, ct);

        await Users.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<User>(Builders<User>.IndexKeys.Ascending(x => x.Username), new CreateIndexOptions { Unique = true })
        }, ct);

        await AuditLogs.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<AuditLog>(Builders<AuditLog>.IndexKeys.Descending(x => x.Timestamp))
        }, ct);
    }

    public async Task<bool> PingAsync(CancellationToken ct = default)
    {
        try
        {
            await Client.GetDatabase("admin").RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: ct);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
