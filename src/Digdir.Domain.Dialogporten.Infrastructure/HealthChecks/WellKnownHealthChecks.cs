using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Digdir.Domain.Dialogporten.Infrastructure.HealthChecks;

internal sealed class WellKnownEndpointsHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WellKnownEndpointsHealthCheck> _logger;
    private readonly IConfiguration _configuration;

    public WellKnownEndpointsHealthCheck(
        IHttpClientFactory httpClientFactory,
        ILogger<WellKnownEndpointsHealthCheck> logger,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var wellKnownEndpoints = _configuration.GetSection("WebApi:Authentication:JwtBearerTokenSchemas")
            .GetChildren()
            .Select(section => section.GetValue<string>("WellKnown"))
            .ToList();

        var client = _httpClientFactory.CreateClient("HealthCheckClient");

        var unhealthyEndpoints = new List<string>();

        foreach (var url in wellKnownEndpoints)
        {
            try
            {
                var response = await client.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Health check failed for Well-Known endpoint: {Url}. Status Code: {StatusCode}", url, response.StatusCode);
                    unhealthyEndpoints.Add($"{url} (Status Code: {response.StatusCode})");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while checking Well-Known endpoint: {Url}", url);
                unhealthyEndpoints.Add($"{url} (Exception: {ex.Message})");
            }
        }

        if (unhealthyEndpoints.Any())
        {
            var description = $"The following endpoints are unhealthy: {string.Join(", ", unhealthyEndpoints)}";
            return HealthCheckResult.Unhealthy(description);
        }

        return HealthCheckResult.Healthy("All Well-Known endpoints are healthy.");

        return HealthCheckResult.Healthy("All Well-Known endpoints are healthy.");
    }
}
