namespace Digdir.Domain.Dialogporten.Domain.SubjectResources;

public sealed class Subject
{
    public long SubjectId { get; set; }
    public string SubjectUrn { get; set; } = null!;

    public List<SubjectResource> SubjectResources { get; set; } = new();
}
