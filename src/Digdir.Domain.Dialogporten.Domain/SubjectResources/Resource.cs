namespace Digdir.Domain.Dialogporten.Domain.SubjectResources;

public sealed class Resource
{
    public long ResourceId { get; set; }
    public string ResourceUrn { get; set; } = null!;

    public List<SubjectResource> SubjectResources { get; set; } = new();
}
