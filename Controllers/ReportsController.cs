using EnterpriseLogIntelligence.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace EnterpriseLogIntelligence.Controllers;

[Authorize]
public sealed class ReportsController : Controller
{
    private readonly ILogRepository _logs;

    public ReportsController(ILogRepository logs) => _logs = logs;

    public IActionResult Index() => View();

    [HttpGet]
    public async Task<IActionResult> Export(CancellationToken ct)
    {
        var from = DateTime.UtcNow.AddDays(-7);
        var to = DateTime.UtcNow;

        var logs = await _logs.GetForAnalyticsAsync(from, to, ct);

        var sb = new StringBuilder();
        sb.AppendLine("Timestamp,Severity,Application,Service,Source,Message,StatusCode");

        foreach (var log in logs.Take(5000))
        {
            sb.AppendLine(string.Join(",",
                Csv(log.Timestamp.ToString("O")),
                Csv(log.Severity),
                Csv(log.Application),
                Csv(log.Service),
                Csv(log.Source),
                Csv(log.Message),
                log.StatusCode));
        }

        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "enterprise-log-report.csv");
    }

    private static string Csv(string value)
        => $"\"{value.Replace("\"", "\"\"")}\"";
}
