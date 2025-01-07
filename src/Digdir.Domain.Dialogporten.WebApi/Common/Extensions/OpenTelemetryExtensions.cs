using Azure.Monitor.OpenTelemetry.Exporter;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Globalization;
using Serilog;
using Serilog.Configuration;
using Serilog.Sinks.OpenTelemetry;

namespace Digdir.Domain.Dialogporten.WebApi.Common.Extensions;

internal static class OpenTelemetryExtensions
{
    public static IServiceCollection AddDialogportenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        if (string.IsNullOrEmpty(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
            return services;

        var otlpProtocol = configuration["OTEL_EXPORTER_OTLP_PROTOCOL"]?.ToLowerInvariant() switch
        {
            "grpc" => OtlpExportProtocol.Grpc,
            "http/protobuf" => OtlpExportProtocol.HttpProtobuf,
            _ => throw new ArgumentException($"Unsupported protocol: {configuration["OTEL_EXPORTER_OTLP_PROTOCOL"]}")
        };

        var endpoint = new Uri(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]!);

        return services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource.AddService(
                    serviceName: configuration["OTEL_SERVICE_NAME"] ?? environment.ApplicationName);
            })
            .WithTracing(tracing =>
            {
                if (environment.IsDevelopment())
                {
                    tracing.SetSampler(new AlwaysOnSampler());
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
                        o.FilterHttpRequestMessage = _ =>
                        {
                            var parentActivity = Activity.Current?.Parent;
                            if (parentActivity != null && parentActivity.Source.Name.Equals("Azure.Core.Http", StringComparison.Ordinal))
                            {
                                return false;
                            }
                            return true;
                        };
                    })
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddNpgsql()
                    .AddFusionCacheInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(endpoint, "/v1/traces");
                        options.Protocol = otlpProtocol;
                    });
            })
            .WithMetrics(metrics =>
            {
                metrics.AddRuntimeInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();

                var appInsightsConnectionString = configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
                if (!string.IsNullOrEmpty(appInsightsConnectionString))
                {
                    metrics.AddAzureMonitorMetricExporter(options =>
                    {
                        options.ConnectionString = appInsightsConnectionString;
                    });
                }
                else
                {
                    metrics.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(endpoint, "/v1/metrics");
                        options.Protocol = otlpProtocol;
                    });
                }
            })
            .Services;
    }

    public static LoggerConfiguration OpenTelemetryOrConsole(this LoggerSinkConfiguration writeTo, IConfiguration? configuration = null)
    {
        const string otelExporterOtlpEndpoint = "OTEL_EXPORTER_OTLP_ENDPOINT";
        const string otelExporterOtlpProtocol = "OTEL_EXPORTER_OTLP_PROTOCOL";
        var otelEndpoint = configuration?[otelExporterOtlpEndpoint] ?? Environment.GetEnvironmentVariable(otelExporterOtlpEndpoint);
        var otelProtocol = configuration?[otelExporterOtlpProtocol] ?? Environment.GetEnvironmentVariable(otelExporterOtlpProtocol);
        return otelEndpoint switch
        {
            null => writeTo.Console(formatProvider: CultureInfo.InvariantCulture),
            not null when Enum.TryParse<OtlpProtocol>(otelProtocol, out var protocol) => writeTo.OpenTelemetry(options =>
            {
                options.Endpoint = otelEndpoint;
                options.Protocol = protocol;
            }),
            _ => throw new InvalidOperationException($"Invalid otel protocol: {otelProtocol}")
        };
    }
}
