using EnterpriseLogIntelligence.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace EnterpriseLogIntelligence.Controllers;
[Authorize]
public sealed class GeneratorController:Controller
{
    private readonly ILogGeneratorService _generator; public GeneratorController(ILogGeneratorService generator)=>_generator=generator;
    public IActionResult Index()=>View();
    [HttpPost][ValidateAntiForgeryToken] public async Task<IActionResult> Generate(int count,CancellationToken ct){var created=await _generator.GenerateAsync(count,ct);TempData["Message"]=$"Generated and ingested {created:N0} log events.";return RedirectToAction(nameof(Index));}
}
