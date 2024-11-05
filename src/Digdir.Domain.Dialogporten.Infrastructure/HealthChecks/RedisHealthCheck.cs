using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
namespace Digdir.Domain.Dialogporten.Infrastructure.HealthChecks;

internal sealed class RedisHealthCheck : IHealthCheck
{
    private readonly InfrastructureSettings _settings;
    private readonly ILogger<RedisHealthCheck> _logger;

    public RedisHealthCheck(IOptions<InfrastructureSettings> options, ILogger<RedisHealthCheck> logger)
    {
        _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var timeout = 15000;

            var options = ConfigurationOptions.Parse(_settings.Redis.ConnectionString);

            options.AsyncTimeout = timeout;
            options.ConnectTimeout = timeout;
            options.SyncTimeout = timeout;

            using var redis = await ConnectionMultiplexer.ConnectAsync(options);
            var db = redis.GetDatabase();
            await db.PingAsync();

            if (sw.Elapsed > TimeSpan.FromSeconds(5))
            {
                _logger.LogWarning("Redis connection is slow ({Elapsed:N1}s).", sw.Elapsed.TotalSeconds);
                return HealthCheckResult.Degraded($"Redis connection is slow ({sw.Elapsed.TotalSeconds:N1}s).");
            }

            return HealthCheckResult.Healthy("Redis connection is healthy.");
        }
        catch (RedisTimeoutException ex)
        {
            _logger.LogWarning("Redis connection timed out ({Elapsed:N1}s).", sw.Elapsed.TotalSeconds);
            return HealthCheckResult.Unhealthy("Redis connection timed out.", exception: ex);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Unable to connect to Redis.");
            return HealthCheckResult.Unhealthy("Unable to connect to Redis.", exception: ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while checking Redis health.");
            return HealthCheckResult.Unhealthy("An unexpected error occurred while checking Redis health.", exception: ex);
        }
        finally
        {
            sw.Stop();
        }
    }
}