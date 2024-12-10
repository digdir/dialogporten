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
using Azure.Monitor.OpenTelemetry.Exporter;

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

                var resourceAttributes = settings.ResourceAttributes;
                if (!string.IsNullOrEmpty(resourceAttributes))
                {
                    try
                    {
                        var attributes = System.Web.HttpUtility.ParseQueryString(
                            resourceAttributes.Replace(',', '&')
                        );
                        foreach (string key in attributes.Keys)
                        {
                            if (!string.IsNullOrEmpty(key))
                            {
                                resourceBuilder.AddAttributes(new[] { new KeyValuePair<string, object>(key, attributes[key] ?? string.Empty) });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            "Failed to parse OTEL_RESOURCE_ATTRIBUTES. Expected format: key1=value1,key2=value2",
                            ex
                        );
                    }
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

            telemetryBuilder
                .WithTracing(tracing =>
                {
                    if (builder.Environment.IsDevelopment())
                    {
                        tracing.SetSampler(new AlwaysOnSampler());
                    }

                    foreach (var source in settings.TraceSources)
                    {
                        tracing.AddSource(source);
                    }

                    tracing
                        .AddAspNetCoreInstrumentation(opts =>
                        {
                            opts.RecordException = true;
                            opts.Filter = httpContext => !httpContext.Request.Path.StartsWithSegments("/health");
                        })
                        .AddHttpClientInstrumentation(o =>
                        {
                            o.RecordException = true;
                            o.FilterHttpRequestMessage = (_) =>
                            {
                                var parentActivity = Activity.Current?.Parent;
                                if (parentActivity != null && parentActivity.Source.Name.Equals("Azure.Core.Http", StringComparison.Ordinal))
                                {
                                    return false;
                                }
                                return true;
                            };
                        })
                        .AddNpgsql()
                        .AddFusionCacheInstrumentation();
                });
        }
        else
        {
            Console.WriteLine("[OpenTelemetry] OTLP exporter not configured - skipping");
        }

        if (!string.IsNullOrEmpty(settings.AppInsightsConnectionString))
        {
            telemetryBuilder.WithMetrics(metrics =>
            {
                metrics.AddRuntimeInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddAzureMonitorMetricExporter();
            });
        }

        return builder;
    }
}

public class TelemetrySettings
{
    private const string MassTransitSource = "MassTransit";
    private const string AzureSource = "Azure.*";

    public string? ServiceName { get; set; }
    public string? Endpoint { get; set; }
    public string? Protocol { get; set; }
    public string? AppInsightsConnectionString { get; set; }
    public string? ResourceAttributes { get; set; }
    public HashSet<string> TraceSources { get; set; } = new()
    {
        AzureSource,
        MassTransitSource
    };
}
