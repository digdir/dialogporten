namespace Digdir.Domain.Dialogporten.Application.Externals.CloudEvents;

public class CloudEvent
{
    public string SpecVersion { get; set; } = "1.0";
    public Guid Id { get; set; }
    public string Type { get; set; }
    public DateTimeOffset Time { get; set; }
    public string Resource { get; set; }
    public string ResourceInstance { get; set; }
    public string Subject { get; set; }
    public string Source { get; set; }

    public IDictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
}
