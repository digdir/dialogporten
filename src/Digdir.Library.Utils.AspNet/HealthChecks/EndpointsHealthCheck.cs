using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Digdir.Library.Utils.AspNet.HealthChecks;

internal sealed class EndpointsHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EndpointsHealthCheck> _logger;
    private readonly List<string> _endpoints;

    public EndpointsHealthCheck(
        IHttpClientFactory httpClientFactory,
        ILogger<EndpointsHealthCheck> logger,
        IOptions<EndpointsHealthCheckOptions> options)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _endpoints = options?.Value?.Endpoints ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("HealthCheckClient");
        var unhealthyEndpoints = new List<string>();

        foreach (var url in _endpoints)
        {
            try
            {
                var response = await client.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Health check failed for endpoint: {Url}. Status Code: {StatusCode}", url, response.StatusCode);
                    unhealthyEndpoints.Add($"{url} (Status Code: {response.StatusCode})");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while checking endpoint: {Url}", url);
                unhealthyEndpoints.Add($"{url} (Exception: {ex.GetType().Name})");
            }
        }

        if (unhealthyEndpoints.Count > 0)
        {
            var description = $"The following endpoints are unhealthy: {string.Join(", ", unhealthyEndpoints)}";
            return HealthCheckResult.Unhealthy(description);
        }

        return HealthCheckResult.Healthy("All endpoints are healthy.");
    }
}

internal sealed class EndpointsHealthCheckOptions
{
    public List<string> Endpoints { get; set; } = new();
}