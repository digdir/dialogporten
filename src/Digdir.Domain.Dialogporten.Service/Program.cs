using System.Globalization;
using Digdir.Domain.Dialogporten.Application;
using Digdir.Domain.Dialogporten.Infrastructure;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using Digdir.Domain.Dialogporten.Service;
using Digdir.Domain.Dialogporten.Service.Common;
using Digdir.Library.Utils.AspNet;
using MassTransit;
using Microsoft.Extensions.DependencyInjection.Extensions;

// Using two-stage initialization to catch startup errors.
var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning()
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces)
    .CreateBootstrapLogger();

try
{
    BuildAndRun(args, telemetryConfiguration);
}
catch (Exception ex) when (ex is not OperationCanceledException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

static void BuildAndRun(string[] args, TelemetryConfiguration telemetryConfiguration)
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .MinimumLevel.Warning()
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces));

    builder.Configuration
        .AddAzureConfiguration(builder.Environment.EnvironmentName)
        .AddLocalConfiguration(builder.Environment);

    builder.ConfigureTelemetry((settings, configuration) =>
    {
        settings.ServiceName = configuration["OTEL_SERVICE_NAME"];
        settings.Endpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        settings.Protocol = configuration["OTEL_EXPORTER_OTLP_PROTOCOL"];
        settings.AppInsightsConnectionString = configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

        var resourceAttributes = configuration["OTEL_RESOURCE_ATTRIBUTES"];
        if (!string.IsNullOrEmpty(resourceAttributes))
        {
            foreach (var attribute in resourceAttributes.Split(','))
            {
                var keyValue = attribute.Split('=', 2);
                if (keyValue.Length == 2)
                {
                    settings.ResourceAttributes[keyValue.First().Trim()] = keyValue.Last().Trim();
                }
            }
        }
    });

    builder.Services
        .AddAzureAppConfiguration()
        .AddApplication(builder.Configuration, builder.Environment)
        .AddInfrastructure(builder.Configuration, builder.Environment)
            .WithPubSubCapabilities<ServiceAssemblyMarker>()
            .AndBusConfiguration(x =>
            {
                foreach (var map in MassTransitApplicationUtils.GetApplicationConsumerMaps())
                {
                    x.TryAddTransient(map.AppConsumerType);
                    x.AddConsumer(map.BusConsumerType, map.BusDefinitionType)
                        .Endpoint(x => x.Name = map.EndpointName);
                }
            })
            .Build()
        .AddTransient<IUser, ServiceUser>()
        .AddAspNetHealthChecks();

    var app = builder.Build();
    app.MapAspNetHealthChecks();
    app.UseHttpsRedirection()
        .UseAzureConfiguration();
    app.Run();
}
