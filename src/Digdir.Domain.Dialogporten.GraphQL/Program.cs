using System.Globalization;
using System.Reflection;
using Digdir.Domain.Dialogporten.Application;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using Digdir.Domain.Dialogporten.GraphQL;
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

    // builder.Host.UseSerilog((context, services, configuration) => configuration
    //     .MinimumLevel.Warning()
    //     .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Fatal)
    //     .ReadFrom.Configuration(context.Configuration)
    //     .ReadFrom.Services(services)
    //     .Enrich.FromLogContext()
    //     .WriteTo(Console.WriteLine()));

    /* TODOS:
     * - Gjør DialogDbContext til internal. Kan måtte gjøre dette prosjektet til et "friend assembly" av infrastructure (internalsVisibleTo)
     *
     *
     *
     */


    builder.Services
        .AddApplication(builder.Configuration, builder.Environment)
        .AddInfrastructure(builder.Configuration, builder.Environment)
        .AddAutoMapper(Assembly.GetExecutingAssembly())
        // .AddApplicationInsightsTelemetry()
        .AddScoped<IUser, LocalDevelopmentUser>()
        .AddGraphQLServer()
            .AddProjections()
            .AddFiltering()
            .AddSorting()
            .RegisterDbContext<DialogDbContext>()
            .AddQueryType<DialogQueries>();

    var app = builder.Build();
    app.MapGraphQL();
    app.Run();
}
