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
    public class AspNetHealthChecksOptions
    {
        public string WellKnownEndpointsConfigurationSectionPath { get; set; } = string.Empty;
    }

    public class JwtBearerTokenSchema
    {
        public string Name { get; set; } = string.Empty;
        public string WellKnown { get; set; } = string.Empty;
    }

    public static IServiceCollection AddAspNetHealthChecks(this IServiceCollection services, IConfiguration configuration, Action<AspNetHealthChecksOptions> configure)
    {
        var options = new AspNetHealthChecksOptions();
        configure(options);

        var wellKnownSchemas = configuration
            .GetSection(options.WellKnownEndpointsConfigurationSectionPath)
            .Get<List<JwtBearerTokenSchema>>();

        var wellKnownEndpoints = wellKnownSchemas?.Select(schema => schema.WellKnown).ToList() ?? new List<string>();

        services.Configure<EndpointsHealthCheckOptions>(opts =>
        {
            opts.Endpoints = wellKnownEndpoints;
        });

        // Register the health checks
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["self"])
            .AddCheck<EndpointsHealthCheck>(
                "Endpoints",
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
