using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;
using Microsoft.Extensions.Options;

namespace Digdir.Domain.Dialogporten.Infrastructure.HealthChecks;

internal sealed class RedisHealthCheck : IHealthCheck
{
    private readonly InfrastructureSettings _settings;

    public RedisHealthCheck(IOptions<InfrastructureSettings> options)
    {
        _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var redis = await ConnectionMultiplexer.ConnectAsync(_settings.Redis.ConnectionString);
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