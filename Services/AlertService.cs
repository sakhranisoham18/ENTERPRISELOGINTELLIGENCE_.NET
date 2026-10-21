using EnterpriseLogIntelligence.Models;
using EnterpriseLogIntelligence.Repositories;

namespace EnterpriseLogIntelligence.Services;

public interface IAlertService
{
    Task<List<Alert>> GetRulesAsync(CancellationToken ct = default);
    Task<List<AlertEvent>> GetEventsAsync(int take = 50, CancellationToken ct = default);
    Task CreateRuleAsync(Alert alert, string username, CancellationToken ct = default);
    Task DeleteRuleAsync(string id, string username, CancellationToken ct = default);
    Task ToggleRuleAsync(string id, string username, CancellationToken ct = default);
}

public interface IAlertEvaluationService
{
    Task EvaluateAsync(CancellationToken ct = default);
}

public sealed class AlertService : IAlertService, IAlertEvaluationService
{
    private readonly IAlertRepository _alerts;
    private readonly ILogRepository _logs;
    private readonly IAuditService _audit;

    public AlertService(IAlertRepository alerts, ILogRepository logs, IAuditService audit)
    {
        _alerts = alerts; _logs = logs; _audit = audit;
    }

    public Task<List<Alert>> GetRulesAsync(CancellationToken ct = default) => _alerts.GetRulesAsync(ct);
    public Task<List<AlertEvent>> GetEventsAsync(int take = 50, CancellationToken ct = default) => _alerts.GetEventsAsync(take, ct);

    public async Task CreateRuleAsync(Alert alert, string username, CancellationToken ct = default)
    {
        alert.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
        alert.CreatedAt = DateTime.UtcNow;
        alert.Severity = NormalizeSeverity(alert.Severity);
        alert.Threshold = Math.Max(1, alert.Threshold);
        alert.WindowMinutes = Math.Clamp(alert.WindowMinutes, 1, 1440);
        alert.CooldownMinutes = Math.Clamp(alert.CooldownMinutes, 1, 1440);
        await _alerts.InsertRuleAsync(alert, ct);
        await _audit.RecordAsync(username, "Alert.RuleCreated", $"Created rule '{alert.Name}' for {alert.Severity}.", ct);
    }

    public async Task DeleteRuleAsync(string id, string username, CancellationToken ct = default)
    {
        await _alerts.DeleteRuleAsync(id, ct);
        await _audit.RecordAsync(username, "Alert.RuleDeleted", $"Deleted alert rule {id}.", ct);
    }

    public async Task ToggleRuleAsync(string id, string username, CancellationToken ct = default)
    {
        var rules = await _alerts.GetRulesAsync(ct);
        var rule = rules.FirstOrDefault(x => x.Id == id);
        if (rule is null) return;
        rule.IsEnabled = !rule.IsEnabled;
        await _alerts.UpdateRuleAsync(rule, ct);
        await _audit.RecordAsync(username, "Alert.RuleToggled", $"Rule '{rule.Name}' is now {(rule.IsEnabled ? "enabled" : "disabled")}.", ct);
    }

    public async Task EvaluateAsync(CancellationToken ct = default)
    {
        var rules = await _alerts.GetRulesAsync(ct);
        var now = DateTime.UtcNow;
        foreach (var rule in rules.Where(x => x.IsEnabled))
        {
            if (rule.LastTriggeredAt.HasValue && now - rule.LastTriggeredAt.Value < TimeSpan.FromMinutes(rule.CooldownMinutes)) continue;
            var from = now.AddMinutes(-rule.WindowMinutes);
            var count = await _logs.CountSearchAsync(null, rule.Severity, null, null, null, from, now, ct);
            if (count < rule.Threshold) continue;

            await _alerts.InsertEventAsync(new AlertEvent
            {
                RuleId = rule.Id,
                RuleName = rule.Name,
                Severity = rule.Severity,
                Occurrences = count,
                WindowMinutes = rule.WindowMinutes,
                TriggeredAt = now,
                Status = "OPEN"
            }, ct);

            rule.LastTriggeredAt = now;
            await _alerts.UpdateRuleAsync(rule, ct);
        }
    }

    private static string NormalizeSeverity(string value)
    {
        var s = (value ?? "ERROR").Trim().ToUpperInvariant();
        return s switch { "WARN" => "WARNING", _ when new[] { "DEBUG", "INFO", "WARNING", "ERROR", "CRITICAL", "SECURITY" }.Contains(s) => s, _ => "ERROR" };
    }
}
