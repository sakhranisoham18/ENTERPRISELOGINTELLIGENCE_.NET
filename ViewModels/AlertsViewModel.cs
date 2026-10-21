using EnterpriseLogIntelligence.Models;

namespace EnterpriseLogIntelligence.ViewModels;

public sealed class AlertsViewModel
{
    public List<Alert> Rules { get; set; } = new();
    public List<AlertEvent> Events { get; set; } = new();
}
