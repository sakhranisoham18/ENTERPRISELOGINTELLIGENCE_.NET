using EnterpriseLogIntelligence.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace EnterpriseLogIntelligence.Controllers;
[Authorize]
public sealed class DashboardController : Controller
{
    private readonly IAnalyticsService _analytics;
    public DashboardController(IAnalyticsService analytics)=>_analytics=analytics;
    public async Task<IActionResult> Index(CancellationToken ct)=>View(await _analytics.BuildDashboardAsync(ct));
}
