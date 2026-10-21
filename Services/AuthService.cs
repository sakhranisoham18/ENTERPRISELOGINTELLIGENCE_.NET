using EnterpriseLogIntelligence.Models;
using EnterpriseLogIntelligence.Repositories;
using Microsoft.Extensions.Configuration;

namespace EnterpriseLogIntelligence.Services;

public interface IAuthService
{
    Task<User?> AuthenticateAsync(string username, string password, CancellationToken ct = default);
    Task EnsureAdminAsync(CancellationToken ct = default);
    Task<List<User>> GetUsersAsync(CancellationToken ct = default);
    Task CreateUserAsync(string username, string password, string role, CancellationToken ct = default);
    Task ToggleUserAsync(string id, CancellationToken ct = default);
}

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHashService _passwords;
    private readonly IAuditLogRepository _audit;
    private readonly IConfiguration _config;

    public AuthService(IUserRepository users, IPasswordHashService passwords, IAuditLogRepository audit, IConfiguration config)
    {
        _users = users; _passwords = passwords; _audit = audit; _config = config;
    }

    public async Task<User?> AuthenticateAsync(string username, string password, CancellationToken ct = default)
    {
        var user = await _users.GetByUsernameAsync(username.Trim(), ct);
        if (user is null || !user.IsActive || !_passwords.Verify(password, user.PasswordHash)) return null;
        user.LastLoginAt = DateTime.UtcNow;
        await _users.UpdateAsync(user, ct);
        return user;
    }

    public async Task EnsureAdminAsync(CancellationToken ct = default)
    {
        var username = _config["Authentication:AdminUsername"] ?? "admin";
        var existing = await _users.GetByUsernameAsync(username, ct);
        if (existing is not null) return;

        var password = _config["Authentication:SeedAdminPassword"];
        if (string.IsNullOrWhiteSpace(password)) password = "admin123";
        await _users.InsertAsync(new User
        {
            Username = username,
            PasswordHash = _passwords.Hash(password),
            Role = "Administrator",
            IsActive = true
        }, ct);
    }

    public Task<List<User>> GetUsersAsync(CancellationToken ct = default) => _users.GetAllAsync(ct);

    public async Task CreateUserAsync(string username, string password, string role, CancellationToken ct = default)
    {
        username = username.Trim();
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Username and password are required.");
        if (await _users.GetByUsernameAsync(username, ct) is not null) throw new InvalidOperationException("Username already exists.");
        role = role is "Administrator" or "Analyst" ? role : "Analyst";
        await _users.InsertAsync(new User { Username = username, PasswordHash = _passwords.Hash(password), Role = role }, ct);
    }

    public async Task ToggleUserAsync(string id, CancellationToken ct = default)
    {
        var users = await _users.GetAllAsync(ct);
        var user = users.FirstOrDefault(x => x.Id == id);
        if (user is null) return;
        if (user.Role == "Administrator" && users.Count(x => x.Role == "Administrator" && x.IsActive) <= 1 && user.IsActive) return;
        user.IsActive = !user.IsActive;
        await _users.UpdateAsync(user, ct);
    }
}
