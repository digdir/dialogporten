namespace Digdir.Library.Utils.AspNet;

public sealed class AspNetUtilitiesSettings
{
    public HealthCheckSettings HealthCheckSettings { get; set; } = new();
}

public sealed class HealthCheckSettings
{
    public List<string> HttpGetEndpointsToCheck { get; set; } = [];
}
