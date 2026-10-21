namespace EnterpriseLogIntelligence.Data;

public sealed class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "EnterpriseLogIntelligence";
}
