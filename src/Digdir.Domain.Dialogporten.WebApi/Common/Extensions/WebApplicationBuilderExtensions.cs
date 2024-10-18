using Azure.Monitor.OpenTelemetry.AspNetCore;
using OpenTelemetry.Trace;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace Digdir.Domain.Dialogporten.WebApi.Common.Extensions;

internal static class WebApplicationBuilderExtensions
{
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
                tracing.AddRedisInstrumentation(options => options.SetVerboseDatabaseStatements = true);
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
