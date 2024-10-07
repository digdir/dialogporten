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
        // TODO: Denne har en sterk avhengighet på infrastruktur registreringen. Fiks
        var client = _httpClientFactory.CreateClient("HealthCheckClient");
        var unhealthyEndpoints = new List<string>();

        foreach (var url in _endpoints)
        {
            try
            {
                // TODO: Kan kanskje paralelliseres? Trengs sannsynligvis ikke...
                var response = await client.GetAsync(url, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    continue;
                }

                _logger.LogWarning("Health check failed for endpoint: {Url}. Status Code: {StatusCode}", url, response.StatusCode);
                unhealthyEndpoints.Add($"{url} (Status Code: {response.StatusCode})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while checking endpoint: {Url}", url);
                unhealthyEndpoints.Add($"{url} (Exception: {ex.GetType().Name})");
            }
        }

        if (unhealthyEndpoints.Count <= 0)
        {
            return HealthCheckResult.Healthy("All endpoints are healthy.");
        }

        var description = $"The following endpoints are unhealthy: {string.Join(", ", unhealthyEndpoints)}";
        return HealthCheckResult.Unhealthy(description);
    }
}
