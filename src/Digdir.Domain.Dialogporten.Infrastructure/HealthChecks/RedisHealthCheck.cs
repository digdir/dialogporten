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
            var timeout = 15000;
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var options = ConfigurationOptions.Parse(_settings.Redis.ConnectionString);

            options.AsyncTimeout = timeout;
            options.ConnectTimeout = timeout;
            options.SyncTimeout = timeout;

            using var redis = await ConnectionMultiplexer.ConnectAsync(options);
            var db = redis.GetDatabase();
            await db.PingAsync();
            sw.Stop();

            if (sw.Elapsed > TimeSpan.FromSeconds(5))
            {
                return HealthCheckResult.Degraded($"Redis connection is slow ({sw.Elapsed.TotalSeconds:N1}s).");
            }

            return HealthCheckResult.Healthy("Redis connection is healthy.");
        }
        catch (RedisTimeoutException ex)
        {
            return HealthCheckResult.Unhealthy("Redis connection timed out.", exception: ex);
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