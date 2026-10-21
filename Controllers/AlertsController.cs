using EnterpriseLogIntelligence.Models;
using EnterpriseLogIntelligence.Services;
using EnterpriseLogIntelligence.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseLogIntelligence.Controllers;

[Authorize]
public sealed class AlertsController : Controller
{
    private readonly IAlertService _service;
    public AlertsController(IAlertService service) => _service = service;

    public async Task<IActionResult> Index(CancellationToken ct) => View(new AlertsViewModel { Rules = await _service.GetRulesAsync(ct), Events = await _service.GetEventsAsync(50, ct) });

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Alert alert, CancellationToken ct)
    {
        await _service.CreateRuleAsync(alert, User.Identity?.Name ?? "admin", ct);
        TempData["Message"] = "Alert rule created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(string id, CancellationToken ct)
    {
        await _service.ToggleRuleAsync(id, User.Identity?.Name ?? "admin", ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        await _service.DeleteRuleAsync(id, User.Identity?.Name ?? "admin", ct);
        return RedirectToAction(nameof(Index));
    }
}
