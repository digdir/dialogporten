using System.Globalization;
using System.Reflection;
using Digdir.Domain.Dialogporten.Application;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using Digdir.Domain.Dialogporten.GraphQL;
using Digdir.Domain.Dialogporten.GraphQL.Common.Authentication;
using Digdir.Domain.Dialogporten.GraphQL.Common.Authorization;
using Digdir.Domain.Dialogporten.Infrastructure;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;

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
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Fatal)
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.ApplicationInsights(
            services.GetRequiredService<TelemetryConfiguration>(),
            TelemetryConverter.Traces));

    /* TODOS:
     * - Gjør DialogDbContext til internal. Kan måtte gjøre dette prosjektet til et "friend assembly" av infrastructure (internalsVisibleTo)
     *
     *
     *
     */


    builder.Services
        // Options setup
        .ConfigureOptions<AuthorizationOptionsSetup>()

        // Clean architecture projects
        .AddApplication(builder.Configuration, builder.Environment)
        .AddInfrastructure(builder.Configuration, builder.Environment)

        // Asp infra
        .AddAutoMapper(Assembly.GetExecutingAssembly())
        .AddApplicationInsightsTelemetry()
        // .AddApplicationInsightsTelemetry()
        .AddScoped<IUser, LocalDevelopmentUser>()

        // Graph QL
        .AddGraphQLServer()
            .AddAuthorization()
            .AddProjections()
            .AddFiltering()
            .AddSorting()
            .RegisterDbContext<DialogDbContext>()
            .AddQueryType<DialogQueries>()
            .Services

        // Auth
        .AddDialogportenAuthentication(builder.Configuration)
        .AddAuthorization();

    var app = builder.Build();

    app.UseJwtSchemeSelector();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapGraphQL().RequireAuthorization();
    app.Run();
}
