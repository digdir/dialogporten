using System.Globalization;
using Digdir.Domain.Dialogporten.ChangeDataCapture;
using Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture;
using Digdir.Domain.Dialogporten.ChangeDataCapture.Common;
using Digdir.Domain.Dialogporten.Domain.Outboxes;
using MassTransit;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;

// TODO: Configure Azure Service Bus connection settings and endpoint exchange
// TODO: Configure Postgres connection settings
// TODO: Improve exceptions thrown in this assembly

// Using two-stage initialization to catch startup errors.
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
        .WriteTo.Conditional(
            condition: _ => builder.Environment.IsDevelopment(),
            configureSink: x => x.Console(formatProvider: CultureInfo.InvariantCulture))
        .WriteTo.ApplicationInsights(
            services.GetRequiredService<TelemetryConfiguration>(),
            TelemetryConverter.Traces));

    builder.Configuration.AddAzureConfiguration(builder.Environment.EnvironmentName);

    builder.Services
        .AddAzureAppConfiguration()
        .AddApplicationInsightsTelemetry()
        .AddHostedService<CdcBackgroundHandler>()
        .AddMassTransit(x =>
        {
            var useInMemoryTransport = builder.Configuration.GetValue<bool>("MassTransit:UseInMemoryTransport");

            if (useInMemoryTransport)
            {
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            }
            else
            {
                // todo: Configure for using Azure Service Bus
            }
        })
        .AddSingleton(_ => new PostgresCdcSSubscriptionOptions
        (
            ConnectionString: builder.Configuration["Infrastructure:DialogDbConnectionString"]!,
            ReplicationSlotName: builder.Configuration["ReplicationSlotName"]!,
            PublicationName: builder.Configuration["PublicationName"]!,
            TableName: builder.Configuration["TableName"]!,
            DataMapper: new OutboxReplicationDataMapper()
        ))
        .AddTransient<ICdcSubscription<OutboxMessage>, PostgresCdcSubscription>()
        .AddTransient<ICdcSink<OutboxMessage>, MassTransitSink>()
        .AddHealthChecks();

    var app = builder.Build();

    app.UseHttpsRedirection()
        .UseSerilogRequestLogging();

    app.UseHealthChecks("/healthz");
    app.Run();
}
