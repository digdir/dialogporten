using Digdir.Domain.Dialogporten.ChangeDataCapture;
using Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture;
using Digdir.Domain.Dialogporten.ChangeDataCapture.Common;
using Digdir.Domain.Dialogporten.Domain.Outboxes;
using MassTransit;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;

// TODO: Configure RabbitMQ connection settings and endpoint exchange
// TODO: Configure Postgres connection settings
// TODO: Improve exceptions thrown in this assembly

// Using two-stage initialization to catch startup errors.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning()
    .Enrich.FromLogContext()
    .WriteTo.Console()
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
            condition: x => builder.Environment.IsDevelopment(), 
            configureSink: x => x.Console())
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
            x.UsingRabbitMq((context, cfg) =>
            {
                const string rabbitMqSection = "RabbitMq";
                cfg.Host(builder.Configuration[$"{rabbitMqSection}:Host"], "/", h =>
                {
                    h.Username(builder.Configuration[$"{rabbitMqSection}:Username"]);
                    h.Password(builder.Configuration[$"{rabbitMqSection}:Password"]);
                });
            });
        })
        .AddSingleton(x => new PostgresCdcSSubscriptionOptions
            (
                ConnectionString: builder.Configuration["Infrastructure:DialogDbConnectionString"]!,
                ReplicationSlotName: builder.Configuration["ReplicationSlotName"]!,
                PublicationName: builder.Configuration["PublicationName"]!,
                TableName: builder.Configuration["TableName"]!,
                DataMapper: new OutboxReplicationDataMapper()
            ))
        .AddTransient<ICdcSubscription<OutboxMessage>, PostgresCdcSubscription>()
        .AddTransient<ICdcSink<OutboxMessage>, MassTransitSink>();

    var app = builder.Build();

    app.UseHttpsRedirection()
        .UseSerilogRequestLogging();

    app.Run();
}
