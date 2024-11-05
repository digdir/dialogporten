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
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(40));

            var sw = System.Diagnostics.Stopwatch.StartNew();
            using var redis = await ConnectionMultiplexer.ConnectAsync(_settings.Redis.ConnectionString);
            var db = redis.GetDatabase();
            await db.PingAsync();
            sw.Stop();

            if (sw.Elapsed > TimeSpan.FromSeconds(5))
            {
                return HealthCheckResult.Degraded($"Redis connection is slow ({sw.Elapsed.TotalSeconds:N1}s).");
            }

            return HealthCheckResult.Healthy("Redis connection is healthy.");
        }
        catch (OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy("Redis health check timed out after 40s.");
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