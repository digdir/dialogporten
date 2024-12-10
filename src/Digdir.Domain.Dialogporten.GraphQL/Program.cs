using System.Globalization;
using System.Reflection;
using Digdir.Domain.Dialogporten.Application;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using Digdir.Domain.Dialogporten.GraphQL.Common;
using Digdir.Domain.Dialogporten.GraphQL.Common.Authentication;
using Digdir.Domain.Dialogporten.GraphQL.Common.Authorization;
using Digdir.Domain.Dialogporten.Infrastructure;
using Digdir.Domain.Dialogporten.Application.Common.Extensions.OptionExtensions;
using Digdir.Domain.Dialogporten.GraphQL;
using Digdir.Library.Utils.AspNet;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using FluentValidation;
using HotChocolate.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

const string DialogportenGraphQLSource = "Dialogporten.GraphQL";

var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
// Using two-stage initialization to catch startup errors.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning()
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces)
    .CreateBootstrapLogger();

try
{
    BuildAndRun(args, telemetryConfiguration);
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

static void BuildAndRun(string[] args, TelemetryConfiguration telemetryConfiguration)
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .MinimumLevel.Warning()
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
        .WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces));

    builder.Configuration
        .AddAzureConfiguration(builder.Environment.EnvironmentName)
        .AddLocalConfiguration(builder.Environment);

    builder.Services
        .AddOptions<GraphQlSettings>()
        .Bind(builder.Configuration.GetSection(GraphQlSettings.SectionName))
        .ValidateFluently()
        .ValidateOnStart();

    var thisAssembly = Assembly.GetExecutingAssembly();

    builder.ConfigureTelemetry((settings, configuration) =>
    {
        settings.ServiceName = configuration["OTEL_SERVICE_NAME"];
        settings.Endpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        settings.Protocol = configuration["OTEL_EXPORTER_OTLP_PROTOCOL"];
        settings.AppInsightsConnectionString = configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
        settings.TraceSources.Add(DialogportenGraphQLSource);

        var resourceAttributes = configuration["OTEL_RESOURCE_ATTRIBUTES"];
        if (!string.IsNullOrEmpty(resourceAttributes))
        {
            foreach (var attribute in resourceAttributes.Split(','))
            {
                var keyValue = attribute.Split('=', 2);
                if (keyValue.Length == 2)
                {
                    settings.ResourceAttributes[keyValue.First().Trim()] = keyValue.Last().Trim();
                }
            }
        }
    });

    builder.Services
        // Options setup
        .ConfigureOptions<AuthorizationOptionsSetup>()

        // Clean architecture projects
        .AddApplication(builder.Configuration, builder.Environment)
        .AddInfrastructure(builder.Configuration, builder.Environment)
            .WithPubCapabilities()
            .Build()
        .AddAutoMapper(Assembly.GetExecutingAssembly())
        .AddHttpContextAccessor()
        .AddScoped<IUser, ApplicationUser>()
        .AddValidatorsFromAssembly(thisAssembly, ServiceLifetime.Transient, includeInternalTypes: true)
        .AddAzureAppConfiguration()

        // Graph QL
        .AddDialogportenGraphQl()

        // Add controllers
        .AddControllers()
            .Services

        // Add health checks with the well-known URLs
        .AddAspNetHealthChecks((x, y) => x.HealthCheckSettings.HttpGetEndpointsToCheck = y
            .GetRequiredService<IOptions<GraphQlSettings>>().Value?
            .Authentication?
            .JwtBearerTokenSchemas?
            .Select(z => z.WellKnown)
            .ToList() ?? [])

        // Auth
        .AddDialogportenAuthentication(builder.Configuration)
        .AddAuthorization()
        .AddHealthChecks();

    if (builder.Environment.IsDevelopment())
    {
        var localDevelopmentSettings = builder.Configuration.GetLocalDevelopmentSettings();
        builder.Services
            .ReplaceSingleton<IUser, LocalDevelopmentUser>(predicate: localDevelopmentSettings.UseLocalDevelopmentUser)
            .ReplaceSingleton<IAuthorizationHandler, AllowAnonymousHandler>(
                predicate: localDevelopmentSettings.DisableAuth);
    }

    var app = builder.Build();

    app.MapAspNetHealthChecks();

    app.UseJwtSchemeSelector()
        .UseAuthentication()
        .UseAuthorization()
        .UseMiddleware<DialogTokenMiddleware>()
        .UseSerilogRequestLogging()
        .UseAzureConfiguration();

    app.MapGraphQL()
        .RequireAuthorization()
        .WithOptions(new GraphQLServerOptions
        {
            EnableSchemaRequests = true,
            Tool =
            {
                Enable = true
            }
        });

    app.Run();
}
