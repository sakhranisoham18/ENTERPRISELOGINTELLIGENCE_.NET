using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EnterpriseLogIntelligence.Models;

public sealed class AlertEvent
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public string RuleId { get; set; } = string.Empty;
    public string RuleName { get; set; } = string.Empty;
    public string Severity { get; set; } = "ERROR";
    public long Occurrences { get; set; }
    public int WindowMinutes { get; set; }
    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "OPEN";
}
