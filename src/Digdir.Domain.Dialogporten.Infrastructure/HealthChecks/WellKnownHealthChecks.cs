using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Digdir.Domain.Dialogporten.Infrastructure.HealthChecks;

public class WellKnownEndpointsHealthCheck : IHealthCheck
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

        foreach (var url in wellKnownEndpoints)
        {
            try
            {
                var response = await client.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Health check failed for Well-Known endpoint: {Url}", url);
                    return HealthCheckResult.Unhealthy($"Well-Known endpoint {url} is unhealthy.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while checking Well-Known endpoint: {Url}", url);
                return HealthCheckResult.Unhealthy($"Exception occurred while checking Well-Known endpoint {url}.");
            }
        }

        return HealthCheckResult.Healthy("All Well-Known endpoints are healthy.");
    }
}
