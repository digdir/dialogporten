using Digdir.Tool.Dialogporten.MigrationVerifier;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.ApplicationInsights(
        TelemetryConfiguration.CreateDefault(),
        TelemetryConverter.Traces)
    .CreateBootstrapLogger();

try
{
    await MigrationVerifier.Verify(Log.Logger);
}
catch (Exception e)
{
    Environment.FailFast("MigrationVerifier failed", e);
}

