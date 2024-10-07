using Digdir.Library.Utils.AspNet.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Digdir.Library.Utils.AspNet;

public sealed class AspNetHealthChecksSettings
{
    public List<string>? HttpGetEndpointsToCheck { get; set; }
}

public static class HealthCheckExtensions
{
    private static void MapHealthCheckEndpoint(WebApplication app, string path, Func<HealthCheckRegistration, bool> predicate)
    {
        app.MapHealthChecks(path, new HealthCheckOptions { Predicate = predicate, ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });
    }

    public static IServiceCollection AddAspNetHealthChecks(this IServiceCollection services, AspNetHealthChecksSettings settings)
    {
        var healthChecks = services.AddHealthChecks();

        healthChecks.AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["self"]);

        if (settings.HttpGetEndpointsToCheck != null && settings.HttpGetEndpointsToCheck.Count > 0)
        {
            services.Configure<EndpointsHealthCheckOptions>(opts =>
            {
                opts.GetEndpoints = settings.HttpGetEndpointsToCheck;
            });

            healthChecks.AddCheck<EndpointsHealthCheck>(
                "Endpoints",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["external"]);
        }

        return services;
    }

    public static WebApplication MapAspNetHealthChecks(this WebApplication app)
    {
        MapHealthCheckEndpoint(app, "/health/startup", check => check.Tags.Contains("dependencies"));
        MapHealthCheckEndpoint(app, "/health/liveness", check => check.Tags.Contains("self"));
        MapHealthCheckEndpoint(app, "/health/readiness", check => check.Tags.Contains("critical"));
        MapHealthCheckEndpoint(app, "/health", check => check.Tags.Contains("dependencies"));
        MapHealthCheckEndpoint(app, "/health/deep", check => check.Tags.Contains("dependencies") || check.Tags.Contains("external"));
        return app;
    }
}
