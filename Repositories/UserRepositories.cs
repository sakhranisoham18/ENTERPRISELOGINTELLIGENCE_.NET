using System.Collections.Concurrent;
using EnterpriseLogIntelligence.Data;
using EnterpriseLogIntelligence.Models;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace EnterpriseLogIntelligence.Repositories;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<List<User>> GetAllAsync(CancellationToken ct = default);
    Task InsertAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
}

public sealed class MongoUserRepository : IUserRepository
{
    private readonly MongoDbContext _db;
    public MongoUserRepository(MongoDbContext db) => _db = db;
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await _db.Users.Find(Builders<User>.Filter.Regex(x => x.Username, new MongoDB.Bson.BsonRegularExpression("^" + Regex.Escape(username) + "$", "i"))).FirstOrDefaultAsync(ct);
    }
    public Task<List<User>> GetAllAsync(CancellationToken ct = default) => _db.Users.Find(Builders<User>.Filter.Empty).SortBy(x => x.Username).ToListAsync(ct);
    public Task InsertAsync(User user, CancellationToken ct = default) => _db.Users.InsertOneAsync(user, cancellationToken: ct);
    public Task UpdateAsync(User user, CancellationToken ct = default) => _db.Users.ReplaceOneAsync(x => x.Id == user.Id, user, cancellationToken: ct);
    public Task DeleteAsync(string id, CancellationToken ct = default) => _db.Users.DeleteOneAsync(x => x.Id == id, ct);
}

public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<string, User> _users = new(StringComparer.OrdinalIgnoreCase);
    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default) => Task.FromResult(_users.Values.FirstOrDefault(x => x.Username.Equals(username, StringComparison.OrdinalIgnoreCase)));
    public Task<List<User>> GetAllAsync(CancellationToken ct = default) => Task.FromResult(_users.Values.OrderBy(x => x.Username).ToList());
    public Task InsertAsync(User user, CancellationToken ct = default) { _users[user.Id] = user; return Task.CompletedTask; }
    public Task UpdateAsync(User user, CancellationToken ct = default) { _users[user.Id] = user; return Task.CompletedTask; }
    public Task DeleteAsync(string id, CancellationToken ct = default) { _users.TryRemove(id, out _); return Task.CompletedTask; }
}
