﻿using Digdir.Domain.Dialogporten.Application;
using Digdir.Domain.Dialogporten.Infrastructure;
using MassTransit;
using System.Reflection;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;

// TODO: Add AppConfiguration and key vault
// TODO: Configure RabbitMQ connection settings 
// TODO: Configure Postgres connection settings
// TODO: Improve exceptions thrown in this assembly

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
    var thisAssembly = Assembly.GetExecutingAssembly();

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Fatal)
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.ApplicationInsights(
            services.GetRequiredService<TelemetryConfiguration>(),
            TelemetryConverter.Traces));
    
    builder.Services
        .AddApplicationInsightsTelemetry()
        .AddMassTransit(x =>
        {
            x.AddConsumers(thisAssembly);
            x.UsingRabbitMq((context, cfg) =>
            {
                const string rabbitMqSection = "RabbitMq";
                cfg.Host(builder.Configuration[$"{rabbitMqSection}:Host"], "/", h => {
                    h.Username(builder.Configuration[$"{rabbitMqSection}:Username"]);
                    h.Password(builder.Configuration[$"{rabbitMqSection}:Password"]);
                });
                cfg.ReceiveEndpoint(thisAssembly.GetName().Name!, x => 
                {
                    x.UseMessageRetry(r => r.Intervals(
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(3),
                        TimeSpan.FromSeconds(10)));
                    // TODO: Add delayed redelivery - but we need a rabbitmq plugin for this
                    //x.UseDelayedRedelivery(r => r.Intervals(
                    //    TimeSpan.FromMinutes(1),
                    //    TimeSpan.FromMinutes(3),
                    //    TimeSpan.FromMinutes(10)));
                    x.SetQuorumQueue();
                    x.ConfigureConsumers(context);
                });
            });
        })
        .AddApplication(builder.Configuration.GetSection(ApplicationSettings.ConfigurationSectionName))
        .AddInfrastructure(builder.Configuration.GetSection(InfrastructureSettings.ConfigurationSectionName), builder.Environment);

    var app = builder.Build();
    app.UseHttpsRedirection();
    app.Run(); 
}