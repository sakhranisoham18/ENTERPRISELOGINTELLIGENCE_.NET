namespace EnterpriseLogIntelligence.ViewModels;
public sealed class AnalyticsViewModel
{
    public List<ChartPoint> Volume { get; set; } = new();
    public List<ChartPoint> Severity { get; set; } = new();
    public List<ChartPoint> Services { get; set; } = new();
}
