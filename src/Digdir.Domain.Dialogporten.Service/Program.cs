using System.Globalization;
using Digdir.Domain.Dialogporten.Application;
using Digdir.Domain.Dialogporten.Infrastructure;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using Digdir.Domain.Dialogporten.Domain;
using Digdir.Domain.Dialogporten.Domain.Common.EventPublisher;
using Digdir.Domain.Dialogporten.Service;
using Digdir.Domain.Dialogporten.Service.Consumers;
using Digdir.Library.Utils.AspNet;
using MassTransit;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning()
    .Enrich.FromLogContext()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .WriteTo.ApplicationInsights(
        TelemetryConfiguration.CreateDefault(),
        TelemetryConverter.Traces)
    .CreateBootstrapLogger();

try
{
    BuildAndRun(args);
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

static void BuildAndRun(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .MinimumLevel.Warning()
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.ApplicationInsights(
            services.GetRequiredService<TelemetryConfiguration>(),
            TelemetryConverter.Traces));

    builder.Configuration
        .AddAzureConfiguration(builder.Environment.EnvironmentName)
        .AddLocalConfiguration(builder.Environment);

    // Generic consumers are not registered through MassTransits assembly
    // scanning, so we need to create domain event handlers for all
    // domain events and register them manually
    var openDomainEventConsumer = typeof(DomainEventConsumer<>);
    var openDomainEventConsumerDefinition = typeof(DomainEventConsumerDefinition<>);
    var domainEventConsumers = DomainExtensions.GetDomainEventTypes()
        .Select(x =>
        (
            consumerType: openDomainEventConsumer.MakeGenericType(x),
            definitionType: openDomainEventConsumerDefinition.MakeGenericType(x))
        )
        .ToArray();

    builder.Services
        .AddApplicationInsightsTelemetry()
        .AddApplication(builder.Configuration, builder.Environment)
        .AddInfrastructure(builder.Configuration, builder.Environment)
            .WithPubSubCapabilities<ServiceAssemblyMarker>()
            .AndBusConfiguration(x =>
            {
                foreach (var (consumer, definition) in domainEventConsumers)
                {
                    x.AddConsumer(consumer, definition);
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
