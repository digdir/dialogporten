using Azure.Monitor.OpenTelemetry.AspNetCore;
using Digdir.Library.Utils.AspNet.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;

namespace Digdir.Library.Utils.AspNet;

public static class AspNetUtilitiesExtensions
{
    private const string MassTransitSource = "MassTransit";

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
        var serviceName = builder.Environment.ApplicationName;
        Console.WriteLine($"[OpenTelemetry] Configuring telemetry for service: {serviceName}");

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: serviceName))
            .WithTracing(tracing =>
            {
                // Enable console exporter for debugging
                tracing.AddConsoleExporter();

                tracing.AddAspNetCoreInstrumentation(opts =>
                {
                    opts.RecordException = true;
                })
                .AddHttpClientInstrumentation(opts =>
                {
                    opts.RecordException = true;
                })
                .AddNpgsql()
                .AddSource(MassTransitSource);

                // Add OTLP exporter with explicit configuration
                tracing.AddOtlpExporter(otlpOptions =>
                {
                    var endpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]
                        ?? "http://otel-collector:4318";
                    Console.WriteLine($"[OpenTelemetry] Using endpoint: {endpoint}");

                    otlpOptions.Endpoint = new Uri(endpoint);
                    otlpOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
                    otlpOptions.ExportProcessorType = ExportProcessorType.Batch;
                    otlpOptions.TimeoutMilliseconds = 30000;
                });
            })
            .WithMetrics(metrics =>
            {
                metrics.AddRuntimeInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                metrics.AddOtlpExporter(otlpOptions =>
                {
                    var endpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]
                        ?? "http://otel-collector:4318";

                    otlpOptions.Endpoint = new Uri(endpoint);
                    otlpOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
                    otlpOptions.ExportProcessorType = ExportProcessorType.Batch;
                    otlpOptions.TimeoutMilliseconds = 30000;
                });
            });

        return builder;
    }
}
