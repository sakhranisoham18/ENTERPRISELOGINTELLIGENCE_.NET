using System.Text.Json;
using EnterpriseLogIntelligence.Models;
using EnterpriseLogIntelligence.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseLogIntelligence.Controllers;

[Authorize]
public sealed class IngestionController : Controller
{
    private readonly ILogIngestionService _service;
    private readonly ILogParserService _parser;
    private readonly IConfiguration _config;
    private readonly IAuditService _audit;

    public IngestionController(ILogIngestionService service, ILogParserService parser, IConfiguration config, IAuditService audit)
    { _service = service; _parser = parser; _config = config; _audit = audit; }

    public IActionResult Index() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        var maxBytes = (_config.GetValue<long?>("Application:MaxUploadBytes") ?? 10 * 1024 * 1024);
        if (file is null || file.Length == 0) { TempData["Error"] = "Choose a log file."; return RedirectToAction(nameof(Index)); }
        if (file.Length > maxBytes) { TempData["Error"] = $"File is too large. Maximum allowed is {maxBytes / (1024 * 1024)} MB."; return RedirectToAction(nameof(Index)); }

        var result = await _parser.ParseAsync(file.OpenReadStream(), file.FileName, ct);
        if (result.Logs.Count == 0)
        {
            TempData["Error"] = "No valid log events were found in the uploaded file.";
            return RedirectToAction(nameof(Index));
        }

        var count = await _service.IngestAsync(result.Logs, ct);
        await _audit.RecordAsync(User.Identity?.Name ?? "system", "Logs.FileUpload", $"Uploaded {file.FileName}; imported {count:N0} events; parser warnings: {result.Warnings.Count}.", ct);
        TempData["Message"] = $"Imported {count:N0} log events from {file.FileName}.";
        if (result.Warnings.Count > 0) TempData["Warning"] = string.Join(" ", result.Warnings.Take(3));
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("api/logs")]
    [AllowAnonymous]
    public async Task<IActionResult> Api([FromBody] JsonElement payload, CancellationToken ct)
    {
        var configuredKey = _config["Ingestion:ApiKey"];
        if (!string.IsNullOrWhiteSpace(configuredKey))
        {
            var supplied = Request.Headers["X-ELI-API-Key"].FirstOrDefault();
            if (!string.Equals(supplied, configuredKey, StringComparison.Ordinal)) return Unauthorized(new { message = "Invalid or missing X-ELI-API-Key." });
        }

        List<LogEntry>? logs = null;
        try
        {
            logs = payload.ValueKind == JsonValueKind.Array
                ? JsonSerializer.Deserialize<List<LogEntry>>(payload.GetRawText())
                : new List<LogEntry> { JsonSerializer.Deserialize<LogEntry>(payload.GetRawText())! };
        }
        catch (JsonException) { return BadRequest(new { message = "Invalid JSON payload." }); }
        if (logs is null || logs.Count == 0) return BadRequest(new { message = "No logs supplied." });
        var accepted = await _service.IngestAsync(logs, ct);
        return Ok(new { accepted, receivedAt = DateTime.UtcNow });
    }
}
