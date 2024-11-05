using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Digdir.Library.Utils.AspNet.HealthChecks;

internal sealed class EndpointsHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EndpointsHealthCheck> _logger;
    private readonly List<string> _endpoints;

    public EndpointsHealthCheck(
        IHttpClientFactory httpClientFactory,
        ILogger<EndpointsHealthCheck> logger,
        IOptions<AspNetUtilitiesSettings> options)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _endpoints = options.Value.HealthCheckSettings.HttpGetEndpointsToCheck;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient();
        var unhealthyEndpoints = new ConcurrentBag<string>();

        var tasks = _endpoints.Select(async url =>
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(40));

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var response = await client.GetAsync(url, cts.Token);
                sw.Stop();

                if (sw.Elapsed > TimeSpan.FromSeconds(5))
                {
                    _logger.LogWarning("Health check response was slow for endpoint: {Url}. Elapsed time: {Elapsed:N1}s", url, sw.Elapsed.TotalSeconds);
                    unhealthyEndpoints.Add($"{url} (Degraded - Response time: {sw.Elapsed.TotalSeconds:N1}s)");
                }
                else if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Health check failed for endpoint: {Url}. Status Code: {StatusCode}", url, response.StatusCode);
                    unhealthyEndpoints.Add($"{url} (Status Code: {response.StatusCode})");
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Health check timed out for endpoint: {Url}", url);
                unhealthyEndpoints.Add($"{url} (Timeout after 40s)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while checking endpoint: {Url}", url);
                unhealthyEndpoints.Add($"{url} (Exception: {ex.GetType().Name})");
            }
        });

        await Task.WhenAll(tasks);

        if (unhealthyEndpoints.IsEmpty)
        {
            return HealthCheckResult.Healthy("All endpoints are healthy.");
        }

        var description = $"The following endpoints are unhealthy: {string.Join(", ", unhealthyEndpoints)}";
        return HealthCheckResult.Unhealthy(description);
    }
}
