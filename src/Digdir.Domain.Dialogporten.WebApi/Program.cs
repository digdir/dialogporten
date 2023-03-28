using Digdir.Domain.Dialogporten.Application;
using Digdir.Domain.Dialogporten.Infrastructure;
using Digdir.Domain.Dialogporten.WebApi;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using System.Text.Json.Serialization;

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
catch (Exception ex)
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

    builder.Configuration.AddAzureConfiguration();

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .MinimumLevel.Warning()
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Fatal)
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.ApplicationInsights(
            services.GetRequiredService<TelemetryConfiguration>(),
            TelemetryConverter.Traces));

    builder.Services
        .AddEndpointsApiExplorer()
        .AddFastEndpoints()
        .AddSwaggerDoc(
            maxEndpointVersion: 1,
            shortSchemaNames: true,
            settings: s =>
            {
                s.Title = "Dialogporten";
                s.DocumentName = "V1.0";
                s.Version = "v1.0";
            })
        .AddApplication(x => builder.Configuration.Bind(nameof(ApplicationSettings), x))
        .AddInfrastructure(x => builder.Configuration.Bind(nameof(InfrastructureSettings), x))
        .AddApplicationInsightsTelemetry();

    var app = builder.Build();

    app.UseHttpsRedirection()
        .UseSerilogRequestLogging()
        .UseProblemDetailsExceptionHandler()
        .UseAuthorization()
        .UseFastEndpoints(x =>
        {
            x.Endpoints.RoutePrefix = "api";
            x.Versioning.Prefix = "v";
            x.Versioning.PrependToRoute = true;
            x.Versioning.DefaultVersion = 1;
            x.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
            x.Errors.ResponseBuilder = ErrorResponseBuilderExtensions.ResponseBuilder;
        })
        .UseOpenApi()
        .UseSwaggerUi3(x => x.ConfigureDefaults());

    app.Run();
}
