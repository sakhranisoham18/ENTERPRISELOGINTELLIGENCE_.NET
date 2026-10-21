using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EnterpriseLogIntelligence.Models;

public sealed class Alert
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public string Name { get; set; } = string.Empty;
    public string Severity { get; set; } = "ERROR";
    public int Threshold { get; set; } = 10;
    public int WindowMinutes { get; set; } = 5;
    public int CooldownMinutes { get; set; } = 15;
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastTriggeredAt { get; set; }
}
