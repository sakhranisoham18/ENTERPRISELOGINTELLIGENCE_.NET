using EnterpriseLogIntelligence.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace EnterpriseLogIntelligence.Controllers;
[Authorize]
public sealed class PatternsController:Controller
{
    private readonly IPatternService _service; public PatternsController(IPatternService service)=>_service=service;
    public async Task<IActionResult> Index(CancellationToken ct)=>View(await _service.GetPatternsAsync(ct));
}
