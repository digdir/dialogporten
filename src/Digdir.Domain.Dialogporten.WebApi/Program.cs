using Digdir.Domain.Dialogporten.Application;
using Digdir.Domain.Dialogporten.Infrastructure;
using Digdir.Domain.Dialogporten.Infrastructure.DomainEvents.Outbox.Dispatcher;
using Digdir.Domain.Dialogporten.WebApi;
using Digdir.Domain.Dialogporten.WebApi.Common;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Infrastructure.Common.Serialization;

// Using two-stage initialization to catch startup errors.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning()
    .Enrich.FromLogContext()
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
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Fatal)
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.ApplicationInsights(
            services.GetRequiredService<TelemetryConfiguration>(),
            TelemetryConverter.Traces));

    builder.Configuration.AddAzureConfiguration(builder.Environment.EnvironmentName);
    builder.Services
        // Temporary configuration for outbox through Web api
        .AddHostedService<OutboxScheduler>()

        .AddAzureAppConfiguration()
        .AddApplicationInsightsTelemetry()
        .AddEndpointsApiExplorer()
        .AddFastEndpoints()
        .AddSwaggerDoc(
            maxEndpointVersion: 1,
            shortSchemaNames: true,
            settings: s =>
            {
                s.Title = "Dialogporten";
                s.DocumentName = "V0.1";
                s.Version = "v0.1";
            })
        .AddApplication(builder.Configuration.GetSection(ApplicationSettings.ConfigurationSectionName))
        .AddInfrastructure(builder.Configuration.GetSection(InfrastructureSettings.ConfigurationSectionName));

    var app = builder.Build();

    app.UseHttpsRedirection()
        .UseSerilogRequestLogging()
        .UseProblemDetailsExceptionHandler()
        .UseAuthorization()
        .UseAzureConfiguration()
        .UseFastEndpoints(x =>
        {
            x.Endpoints.RoutePrefix = "api";
            x.Versioning.Prefix = "v";
            x.Versioning.PrependToRoute = true;
            x.Versioning.DefaultVersion = 1;

            // TODO! Do we want this for numerics (ie. zero)?
            // TODO! Determine if this is desirable for all endpoints and all properties with regards to discoverability
            x.Serializer.Options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
            x.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
            x.Errors.ResponseBuilder = ErrorResponseBuilderExtensions.ResponseBuilder;
        })
        .UseOpenApi()
        .UseSwaggerUi3(x => x.ConfigureDefaults());

    app.Run();
}
