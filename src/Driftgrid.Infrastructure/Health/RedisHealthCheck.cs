using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace Driftgrid.Infrastructure.Health;

public sealed class RedisHealthCheck(
    IConnectionMultiplexer redis) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (!redis.IsConnected)
        {
            return HealthCheckResult.Unhealthy(
                "Redis is not connected.");
        }

        await redis.GetDatabase()
            .PingAsync()
            .WaitAsync(cancellationToken);

        return HealthCheckResult.Healthy(
            "Redis is responding.");
    }
}