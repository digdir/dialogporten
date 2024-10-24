using Azure.Monitor.OpenTelemetry.AspNetCore;
using Digdir.Library.Utils.AspNet.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Npgsql;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace Digdir.Library.Utils.AspNet;

public static class AspNetUtilitiesExtensions
{
    public static IServiceCollection AddAspNetHealthChecks(this IServiceCollection services, Action<AspNetUtilitiesSettings, IServiceProvider>? configure = null)
    {
        var optionsBuilder = services.AddOptions<AspNetUtilitiesSettings>();

        if (configure is not null)
        {
            optionsBuilder.Configure(configure);
        }

        return services
            .AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["self"])
            .AddCheck<EndpointsHealthCheck>(
                "Endpoints",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["external"])
            .Services;
    }

    public static WebApplication MapAspNetHealthChecks(this WebApplication app) =>
        app.MapHealthCheckEndpoint("/health/startup", check => check.Tags.Contains("dependencies"))
            .MapHealthCheckEndpoint("/health/liveness", check => check.Tags.Contains("self"))
            .MapHealthCheckEndpoint("/health/readiness", check => check.Tags.Contains("critical"))
            .MapHealthCheckEndpoint("/health", check => check.Tags.Contains("dependencies"))
            .MapHealthCheckEndpoint("/health/deep", check => check.Tags.Contains("dependencies") || check.Tags.Contains("external"));

    private static WebApplication MapHealthCheckEndpoint(this WebApplication app, string path, Func<HealthCheckRegistration, bool> predicate)
    {
        app.MapHealthChecks(path, new HealthCheckOptions { Predicate = predicate, ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });
        return app;
    }

    public static WebApplicationBuilder ConfigureTelemetry(this WebApplicationBuilder builder)
    {
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: builder.Environment.ApplicationName))
            .WithTracing(tracing =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    tracing.SetSampler(new AlwaysOnSampler());
                }

                tracing.AddAspNetCoreInstrumentation(options =>
                {
                    options.Filter = (httpContext) =>
                        !httpContext.Request.Path.StartsWithSegments("/health");
                });

                tracing.AddHttpClientInstrumentation();
                tracing.AddNpgsql();
                tracing.AddSource(DiagnosticHeaders.DefaultListenerName); // MassTransit ActivitySource
            })
            .WithMetrics(metrics =>
            {
                metrics.AddRuntimeInstrumentation();
            });

        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
        {
            builder.Services.AddOpenTelemetry().UseAzureMonitor();
        }
        else
        {
            // Use Application Insights SDK for local development
            builder.Services.AddApplicationInsightsTelemetry();
        }

        return builder;
    }
}
