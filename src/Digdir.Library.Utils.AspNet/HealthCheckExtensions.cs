using Digdir.Library.Utils.AspNet.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Digdir.Library.Utils.AspNet;

public static class HealthCheckExtensions
{
    public class AspNetHealthChecksSettings
    {
        public List<string>? HttpGetEndpointsToCheck { get; set; }
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
        app.MapHealthChecks("/health/startup", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("dependencies"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        app.MapHealthChecks("/health/liveness", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("self"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        app.MapHealthChecks("/health/readiness", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("critical"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("dependencies"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        app.MapHealthChecks("/health/deep", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("dependencies") || check.Tags.Contains("external"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        return app;
    }
}
