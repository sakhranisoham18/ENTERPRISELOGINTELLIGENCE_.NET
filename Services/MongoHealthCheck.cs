using Microsoft.Extensions.Diagnostics.HealthChecks;
using EnterpriseLogIntelligence.Data;

namespace EnterpriseLogIntelligence.Services;

public sealed class MongoHealthCheck : IHealthCheck
{
    private readonly MongoDbContext _mongo;
    private readonly IConfiguration _config;
    public MongoHealthCheck(MongoDbContext mongo, IConfiguration config) { _mongo = mongo; _config = config; }
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (!string.Equals(_config["Storage:Provider"], "MongoDB", StringComparison.OrdinalIgnoreCase))
            return HealthCheckResult.Healthy("In-memory storage mode is active.");
        return await _mongo.PingAsync(cancellationToken)
            ? HealthCheckResult.Healthy("MongoDB connection is healthy.")
            : HealthCheckResult.Unhealthy("MongoDB is configured but unreachable.");
    }
}
