using EnterpriseLogIntelligence.Repositories;
using EnterpriseLogIntelligence.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseLogIntelligence.Controllers;

[Authorize]
public sealed class LogsController : Controller
{
    private readonly ILogRepository _logs;
    public LogsController(ILogRepository logs) => _logs = logs;

    public async Task<IActionResult> Index(LogFilterViewModel vm, CancellationToken ct)
    {
        vm.Page = Math.Max(1, vm.Page); vm.PageSize = Math.Clamp(vm.PageSize, 10, 100);
        var skip = (vm.Page - 1) * vm.PageSize;
        vm.Total = await _logs.CountSearchAsync(vm.Search, vm.Severity, vm.Service, vm.Application, vm.Environment, vm.From, vm.To, ct);
        vm.Results = (await _logs.SearchAsync(vm.Search, vm.Severity, vm.Service, vm.Application, vm.Environment, vm.From, vm.To, skip, vm.PageSize, ct)).Select(Map).ToList();
        return View(vm);
    }

    public async Task<IActionResult> Details(string id, CancellationToken ct)
    {
        var log = await _logs.GetByIdAsync(id, ct);
        return log is null ? NotFound() : View(log);
    }

    private static LogRowViewModel Map(Models.LogEntry x) => new() { Id = x.Id, Timestamp = x.Timestamp, Severity = x.Severity, Service = x.Service, Message = x.Message };
}
