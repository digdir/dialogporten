using System.Globalization;
using System.Reflection;
using Digdir.Domain.Dialogporten.Application;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using Digdir.Domain.Dialogporten.GraphQL;
using Digdir.Domain.Dialogporten.Infrastructure;
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

    builder.Services
        .AddApplication(builder.Configuration, builder.Environment)
        .AddInfrastructure(builder.Configuration, builder.Environment)
        .AddAutoMapper(Assembly.GetExecutingAssembly())
        // .AddApplicationInsightsTelemetry()
        .AddScoped<IUser, LocalDevelopmentUser>()
        .AddGraphQLServer()
        .AddQueryType<DialogQueries>();

    var app = builder.Build();
    app.MapGraphQL();
    app.Run();
}
