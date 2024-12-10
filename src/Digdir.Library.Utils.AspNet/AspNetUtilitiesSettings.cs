namespace Digdir.Library.Utils.AspNet;

public sealed class AspNetUtilitiesSettings
{
    public HealthCheckSettings HealthCheckSettings { get; set; } = new();
}

public sealed class HealthCheckSettings
{
    public List<string> HttpGetEndpointsToCheck { get; set; } = [];
}

public sealed class TelemetrySettings
{
    private const string MassTransitSource = "MassTransit";
    private const string AzureSource = "Azure.*";

    public string? ServiceName { get; set; }
    public string? Endpoint { get; set; }
    public string? Protocol { get; set; }
    public string? AppInsightsConnectionString { get; set; }
    public string? ResourceAttributes { get; set; }
    public HashSet<string> TraceSources { get; set; } = new()
    {
        AzureSource,
        MassTransitSource
    };
}