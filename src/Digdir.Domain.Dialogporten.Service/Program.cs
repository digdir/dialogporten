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
using MassTransit;

// TODO: Add AppConfiguration and key vault
// TODO: Configure Service bus connection settings
// TODO: Configure Postgres connection settings
// TODO: Improve exceptions thrown in this assembly

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

    builder.Configuration.AddLocalConfiguration(builder.Environment);

    var openDomainEventConsumer = typeof(DomainEventConsumer<>);
    var openDomainEventConsumerDefinition = typeof(DomainEventConsumerDefinition<>);
    var domainEventConsumers = DomainAssemblyMarker.Assembly
        .GetTypes()
        .Where(x => !x.IsAbstract && !x.IsInterface && !x.IsGenericType)
        .Where(x => x.IsAssignableTo(typeof(IDomainEvent)))
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
        .AddHealthChecks();

    var app = builder.Build();
    app.UseHttpsRedirection();
    app.MapHealthChecks("/healthz");
    app.Run();
}
