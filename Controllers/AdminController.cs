using EnterpriseLogIntelligence.Data;
using EnterpriseLogIntelligence.Services;
using EnterpriseLogIntelligence.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseLogIntelligence.Controllers;

[Authorize(Roles = "Administrator")]
public sealed class AdminController : Controller
{
    private readonly IAuthService _auth;
    private readonly IAuditService _audit;
    private readonly MongoDbContext _mongo;
    private readonly IConfiguration _config;

    public AdminController(IAuthService auth, IAuditService audit, MongoDbContext mongo, IConfiguration config)
    { _auth = auth; _audit = audit; _mongo = mongo; _config = config; }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var provider = _config["Storage:Provider"] ?? "InMemory";
        return View(new AdminViewModel
        {
            StorageProvider = provider,
            MongoConnected = provider.Equals("MongoDB", StringComparison.OrdinalIgnoreCase) && await _mongo.PingAsync(ct),
            DatabaseName = _config["MongoDb:DatabaseName"] ?? "EnterpriseLogIntelligence",
            IngestionApiKeyConfigured = !string.IsNullOrWhiteSpace(_config["Ingestion:ApiKey"]),
            Users = await _auth.GetUsersAsync(ct),
            AuditLogs = await _audit.GetRecentAsync(100, ct)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(string username, string password, string role, CancellationToken ct)
    {
        try
        {
            await _auth.CreateUserAsync(username, password, role, ct);
            await _audit.RecordAsync(User.Identity?.Name ?? "admin", "Admin.UserCreated", $"Created user '{username}'.", ct);
            TempData["Message"] = "User created successfully.";
        }
        catch (Exception ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUser(string id, CancellationToken ct)
    {
        await _auth.ToggleUserAsync(id, ct);
        await _audit.RecordAsync(User.Identity?.Name ?? "admin", "Admin.UserToggled", $"Toggled user {id}.", ct);
        return RedirectToAction(nameof(Index));
    }
}
