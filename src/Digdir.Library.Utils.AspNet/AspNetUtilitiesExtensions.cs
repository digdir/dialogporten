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

    public static WebApplicationBuilder ConfigureTelemetry(
        this WebApplicationBuilder builder,
        Action<TelemetrySettings, IConfiguration>? configure = null)
    {
        var serviceName = builder.Environment.ApplicationName;
        Console.WriteLine($"[OpenTelemetry] Configuring telemetry for service: {serviceName}");

        var settings = new TelemetrySettings();
        configure?.Invoke(settings, builder.Configuration);

        var telemetryBuilder = builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: serviceName));

        // Only configure OTLP if both endpoint and protocol are specified
        if (!string.IsNullOrEmpty(settings.Endpoint) && !string.IsNullOrEmpty(settings.Protocol))
        {
            Console.WriteLine($"[OpenTelemetry] Using endpoint: {settings.Endpoint}");
            Console.WriteLine($"[OpenTelemetry] Using protocol: {settings.Protocol}");

            var protocol = settings.Protocol.ToLowerInvariant() switch
            {
                "grpc" => OtlpExportProtocol.Grpc,
                "http/protobuf" => OtlpExportProtocol.HttpProtobuf,
                _ => throw new ArgumentException($"Unsupported protocol: {settings.Protocol}")
            };

            telemetryBuilder.UseOtlpExporter(protocol, new Uri(settings.Endpoint));
        }
        else
        {
            Console.WriteLine("[OpenTelemetry] OTLP exporter not configured - skipping");
        }

        telemetryBuilder
            .WithTracing(tracing =>
            {
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
            })
            .WithMetrics(metrics =>
            {
                metrics.AddRuntimeInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        return builder;
    }
}

public class TelemetrySettings
{
    public string? Endpoint { get; set; }
    public string? Protocol { get; set; }
}
