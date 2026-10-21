using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EnterpriseLogIntelligence.Models;

public sealed class LogPattern
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public string Name { get; set; } = string.Empty;
    public string Severity { get; set; } = "ERROR";
    public string Service { get; set; } = string.Empty;
    public long Occurrences { get; set; }
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
}
