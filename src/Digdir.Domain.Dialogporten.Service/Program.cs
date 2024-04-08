using System.Globalization;
using Digdir.Domain.Dialogporten.Application;
using Digdir.Domain.Dialogporten.Infrastructure;
using MassTransit;
using System.Reflection;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using Digdir.Domain.Dialogporten.Service;

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
    var thisAssembly = Assembly.GetExecutingAssembly();

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Fatal)
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Conditional(
            condition: _ => builder.Environment.IsDevelopment(),
            configureSink: x => x.Console(formatProvider: CultureInfo.InvariantCulture))
        .WriteTo.ApplicationInsights(
            services.GetRequiredService<TelemetryConfiguration>(),
            TelemetryConverter.Traces));

    builder.Services
        .AddApplicationInsightsTelemetry()
        .AddMassTransit(x =>
        {
            x.AddConsumers(thisAssembly);

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
        .AddApplication(builder.Configuration, builder.Environment)
        .AddInfrastructure(builder.Configuration, builder.Environment)
        .AddTransient<IUser, ServiceUser>()
        .AddHealthChecks();

    var app = builder.Build();
    app.UseHttpsRedirection();
    app.MapHealthChecks("/healthz");
    app.Run();
}
