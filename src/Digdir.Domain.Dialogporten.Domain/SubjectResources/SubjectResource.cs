namespace Digdir.Domain.Dialogporten.Domain.SubjectResources;

public sealed class SubjectResource
{
    public long SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    public long ResourceId { get; set; }
    public Resource Resource { get; set; } = null!;
}
