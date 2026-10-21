namespace EnterpriseLogIntelligence.ViewModels;

public sealed class LogFilterViewModel
{
    public string? Search { get; set; }
    public string? Severity { get; set; }
    public string? Service { get; set; }
    public string? Application { get; set; }
    public string? Environment { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public long Total { get; set; }
    public List<LogRowViewModel> Results { get; set; } = new();
}
