using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EnterpriseLogIntelligence.Models;

public sealed class AuditLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Username { get; set; } = "system";
    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}
