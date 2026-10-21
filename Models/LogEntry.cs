using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EnterpriseLogIntelligence.Models;

public sealed class LogEntry
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Severity { get; set; } = "INFO";
    public string Application { get; set; } = "WebPortal";
    public string Service { get; set; } = "API";
    public string Source { get; set; } = "ApplicationServer";
    public string Message { get; set; } = string.Empty;
    public string IpAddress { get; set; } = "127.0.0.1";
    public string RequestId { get; set; } = Guid.NewGuid().ToString("N")[..12];
    public int StatusCode { get; set; } = 200;
    public string Environment { get; set; } = "Development";
    public Dictionary<string, string> Metadata { get; set; } = new();
}
