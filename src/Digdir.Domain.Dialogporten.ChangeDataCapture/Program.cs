using System.Globalization;
using Digdir.Domain.Dialogporten.ChangeDataCapture;
using Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Checkpoints;
using Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Mappers;
using Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Subscriptions;
using Digdir.Domain.Dialogporten.ChangeDataCapture.Common.Extensions;
using Digdir.Domain.Dialogporten.ChangeDataCapture.Outbox;
using Digdir.Domain.Dialogporten.Domain.Outboxes;
using MassTransit;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Options;
using Npgsql;
using Serilog;

// Using two-stage initialization to catch startup errors.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning()
    //.MinimumLevel.Override("Digdir.Domain.Dialogporten.ChangeDataCapture", Serilog.Events.LogEventLevel.Debug)
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
        //.MinimumLevel.Override("Digdir.Domain.Dialogporten.ChangeDataCapture", Serilog.Events.LogEventLevel.Debug)
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
        // Options
        .AddOptions<OutboxCdcSubscriptionOptions>()
            .BindConfiguration(OutboxCdcSubscriptionOptions.SectionName)
            .Configure<IConfiguration>((option, conf) => option.ConnectionString ??= conf["Infrastructure:DialogDbConnectionString"]!)
            .Services

        // Infrastructure
        .AddAzureAppConfiguration()
        .AddApplicationInsightsTelemetry()
        .AddHostedService<CheckpointSynchronizer>()
        .AddHostedService<OutboxCdcBackgroundHandler>()
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
        .AddHealthChecks()
            .Services

        // Singleton
        .AddSingleton(x => NpgsqlDataSource.Create(x.GetRequiredService<IOptions<OutboxCdcSubscriptionOptions>>().Value.ConnectionString))
        .AddSingleton<ICheckpointCache, CheckpointCache>()

        // Scoped

        // Transient
        .AddTransient<IOutboxReaderRepository, OutboxReaderRepository>()
        .AddTransient<ICheckpointRepository, CheckpointRepository>()
        .AddTransient<ISubscriptionRepository, SubscriptionRepository>()
        //.AddTransient(typeof(IReplicationDataMapper<>), typeof(DynamicReplicationDataMapper<>))
        .AddTransient<IReplicationMapper<OutboxMessage>, OutboxReplicationMapper>()
        .AddTransient<ICdcSubscription<OutboxMessage>, OutboxCdcSubscription>()
        .AddTransient<ICdcSink<OutboxMessage>, ConsoleSink>();

    var app = builder.Build();
    app.UseHttpsRedirection()
        .UseSerilogRequestLogging()
        .UseHealthChecks("/healthz");
    app.Run();
}
