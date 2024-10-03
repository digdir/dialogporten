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
    public static IServiceCollection AddAspNetHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["self"])
            .AddCheck<WellKnownEndpointsHealthCheck>(
                "Well-Known Endpoints",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["dependencies"]);

        return services;
    }

    public static void MapAspNetHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/startup", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("dependencies"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        app.MapHealthChecks("/liveness", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("self"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        app.MapHealthChecks("/readiness", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("critical"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("dependencies"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
    }
}
