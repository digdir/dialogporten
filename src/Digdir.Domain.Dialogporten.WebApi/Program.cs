using Digdir.Domain.Dialogporten.Application;
using Digdir.Domain.Dialogporten.Infrastructure;
using Digdir.Domain.Dialogporten.Infrastructure.DomainEvents.Outbox.Dispatcher;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Collections;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using Digdir.Domain.Dialogporten.WebApi.Common.Json;
using Digdir.Domain.Dialogporten.WebApi.Common.Options;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using FluentValidation;
using System.Reflection;
using Digdir.Domain.Dialogporten.WebApi.Common.OptionExtensions;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using Digdir.Domain.Dialogporten.WebApi.Common;

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
        .AddOptions<AuthenticationOptions>()
        .Bind(builder.Configuration.GetSection(AuthenticationOptions.SectionName))
        .ValidateFluently()
        .ValidateOnStart();

    if (!builder.Environment.IsDevelopment())
    {
        // Temporary configuration for outbox through Web api
        builder.Services.AddHostedService<OutboxScheduler>();
    }

    var thisAssembly = Assembly.GetExecutingAssembly();
    builder.Services
        // Configuration
        .Configure<AuthenticationOptions>(builder.Configuration.GetSection(AuthenticationOptions.SectionName))

        // Options
        .ConfigureOptions<AuthorizationOptionsSetup>()

        // Clean architecture projects
        .AddApplication(builder.Configuration.GetSection(ApplicationSettings.ConfigurationSectionName))
        .AddInfrastructure(builder.Configuration.GetSection(InfrastructureSettings.ConfigurationSectionName), builder.Environment)

        // Asp infrastructure
        .AddScoped<IUser, ApplicationUser>()
        .AddHttpContextAccessor()
        .AddValidatorsFromAssembly(thisAssembly, includeInternalTypes: true)
        .AddAzureAppConfiguration()
        .AddApplicationInsightsTelemetry()
        .AddEndpointsApiExplorer()
        .AddFastEndpoints()
        .SwaggerDocument(x =>
        {
            x.MaxEndpointVersion = 1;
            x.ShortSchemaNames = true;
            x.DocumentSettings = s =>
            {
                s.Title = "Dialogporten";
                s.DocumentName = "V0.1";
                s.Version = "v0.1";
            };
        })
        .AddControllers(options => options.InputFormatters.Insert(0, JsonPatchInputFormatter.Get()))
            .AddNewtonsoftJson()
            .Services

        // Auth
        .AddDialogportenAuthentication(builder.Configuration)
        .AddAuthorization();

    var app = builder.Build();

    app.UseHttpsRedirection()
        .UseSerilogRequestLogging()
        .UseProblemDetailsExceptionHandler()
        .UseAuthentication()
        .UseAuthorization()
        .UseAzureConfiguration()
        .UseFastEndpoints(x =>
        {
            x.Endpoints.RoutePrefix = "api";
            x.Versioning.Prefix = "v";
            x.Versioning.PrependToRoute = true;
            x.Versioning.DefaultVersion = 1;
            x.Serializer.Options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            // Do not serialize empty collections
            x.Serializer.Options.TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { IgnoreEmptyCollections }
            };
            x.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
            x.Serializer.Options.Converters.Add(new UtcDateTimeOffsetConverter());
            x.Serializer.Options.Converters.Add(new DateTimeNotSupportedConverter());
            x.Errors.ResponseBuilder = ErrorResponseBuilderExtensions.ResponseBuilder;
        })
        .UseOpenApi()
        .UseSwaggerUi3(x => x.ConfigureDefaults());
    app.MapControllers();
    app.Run();
}

static void IgnoreEmptyCollections(JsonTypeInfo type_info)
{
    foreach (var property in type_info.Properties)
    {
        if (property.PropertyType.IsAssignableTo(typeof(ICollection)))
        {
            property.ShouldSerialize = (_, val) => val is ICollection collection && collection.Count > 0;
        }
    }
}
