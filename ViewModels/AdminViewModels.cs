using EnterpriseLogIntelligence.Models;

namespace EnterpriseLogIntelligence.ViewModels;

public sealed class AdminViewModel
{
    public string StorageProvider { get; set; } = "InMemory";
    public bool MongoConnected { get; set; }
    public string DatabaseName { get; set; } = string.Empty;
    public bool IngestionApiKeyConfigured { get; set; }
    public List<User> Users { get; set; } = new();
    public List<AuditLog> AuditLogs { get; set; } = new();
}
