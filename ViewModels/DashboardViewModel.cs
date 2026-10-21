namespace EnterpriseLogIntelligence.ViewModels;

public sealed class DashboardViewModel
{
    public long TotalLogs { get; set; }
    public long Errors { get; set; }
    public long Warnings { get; set; }
    public long Critical { get; set; }
    public double ErrorRate { get; set; }
    public double IngestionRate { get; set; }
    public List<ChartPoint> Volume { get; set; } = new();
    public List<ChartPoint> Severity { get; set; } = new();
    public List<ChartPoint> Services { get; set; } = new();
    public List<PatternViewModel> Patterns { get; set; } = new();
    public List<LogRowViewModel> RecentCritical { get; set; } = new();
}
public sealed class ChartPoint { public string Label { get; set; } = ""; public double Value { get; set; } }
public sealed class PatternViewModel { public string Name { get; set; } = ""; public long Occurrences { get; set; } public string Severity { get; set; } = ""; public string Service { get; set; } = ""; }
public sealed class LogRowViewModel { public string Id { get; set; } = ""; public DateTime Timestamp { get; set; } public string Severity { get; set; } = ""; public string Service { get; set; } = ""; public string Message { get; set; } = ""; }
