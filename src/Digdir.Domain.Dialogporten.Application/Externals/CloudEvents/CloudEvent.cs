namespace Digdir.Domain.Dialogporten.Application.Externals.CloudEvents;

public class CloudEvent
{
    public string SpecVersion { get; set; } = "1.0";
    public Guid Id { get; set; }
    public string Type { get; set; } = null!;
    public DateTimeOffset Time { get; set; }
    public string Resource { get; set; } = null!;
    public string ResourceInstance { get; set; } = null!;
    public string? Subject { get; set; }
    public string? AlternativeSubject { get; set; }
    public string Source { get; set; } = null!;
    public IDictionary<string, object> Data { get; set; } = null!;
}
