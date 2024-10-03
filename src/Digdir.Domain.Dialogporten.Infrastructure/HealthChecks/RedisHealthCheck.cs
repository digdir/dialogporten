using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace Digdir.Domain.Dialogporten.Infrastructure.HealthChecks;

internal sealed class RedisHealthCheck : IHealthCheck
{
    private readonly string _connectionString;

    public RedisHealthCheck(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var redis = await ConnectionMultiplexer.ConnectAsync(_connectionString);
            var db = redis.GetDatabase();
            await db.PingAsync();
            return HealthCheckResult.Healthy("Redis connection is healthy.");
        }
        catch (RedisConnectionException ex)
        {
            return HealthCheckResult.Unhealthy("Unable to connect to Redis.", exception: ex);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("An unexpected error occurred while checking Redis health.", exception: ex);
        }
    }
}