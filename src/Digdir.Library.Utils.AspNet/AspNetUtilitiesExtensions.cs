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
using System.Diagnostics;

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
        var settings = new TelemetrySettings();
        configure?.Invoke(settings, builder.Configuration);

        Console.WriteLine($"[OpenTelemetry] Configuring telemetry for service: {settings.ServiceName}");
        foreach (var attr in settings.ResourceAttributes)
        {
            Console.WriteLine($"[OpenTelemetry] Resource attribute: {attr.Key}={attr.Value}");
        }

        var telemetryBuilder = builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                var resourceBuilder = resource.AddService(serviceName: settings.ServiceName ?? builder.Environment.ApplicationName);

                foreach (var attr in settings.ResourceAttributes)
                {
                    resourceBuilder.AddAttributes(new[] { new KeyValuePair<string, object>(attr.Key, attr.Value) });
                }
            });

        if (!string.IsNullOrEmpty(settings.Endpoint) && !string.IsNullOrEmpty(settings.Protocol))
        {
            Console.WriteLine($"[OpenTelemetry] Using endpoint: {settings.Endpoint}");
            Console.WriteLine($"[OpenTelemetry] Using protocol: {settings.Protocol}");

            var otlpProtocol = settings.Protocol.ToLowerInvariant() switch
            {
                "grpc" => OtlpExportProtocol.Grpc,
                "http/protobuf" => OtlpExportProtocol.HttpProtobuf,
                "http" => OtlpExportProtocol.HttpProtobuf,
                _ => throw new ArgumentException($"Unsupported protocol: {settings.Protocol}")
            };

            telemetryBuilder.UseOtlpExporter(otlpProtocol, new Uri(settings.Endpoint));
        }
        else
        {
            Console.WriteLine("[OpenTelemetry] OTLP exporter not configured - skipping");
        }

        telemetryBuilder
            .WithTracing(tracing =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    tracing.SetSampler(new AlwaysOnSampler());
                }

                tracing.AddSource("Azure.*")
                    .AddAspNetCoreInstrumentation(opts =>
                    {
                        opts.RecordException = true;
                        opts.Filter = httpContext => !httpContext.Request.Path.StartsWithSegments("/health");
                    })
                    .AddHttpClientInstrumentation(o => o.FilterHttpRequestMessage = (_) =>
                    {
                        o.RecordException = true;
                        // Azure SDKs create their own client span before calling the service using HttpClient
                        // In this case, we would see two spans corresponding to the same operation
                        // 1) created by Azure SDK 2) created by HttpClient
                        // To prevent this duplication we are filtering the span from HttpClient
                        // as span from Azure SDK contains all relevant information needed.
                        var parentActivity = Activity.Current?.Parent;
                        if (parentActivity != null && parentActivity.Source.Name.Equals("Azure.Core.Http", StringComparison.Ordinal))
                        {
                            return false;
                        }
                        return true;
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
    public string? ServiceName { get; set; }
    public string? Endpoint { get; set; }
    public string? Protocol { get; set; }
    public Dictionary<string, string> ResourceAttributes { get; set; } = new();
}
