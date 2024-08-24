namespace Digdir.Tool.Dialogporten.SubjectResourceSync.External.Altinn;

internal sealed class UpdatedSubjectResourceDto
{
    public string SubjectUrn { get; set; } = null!;
    public string ResourceUrn { get; set; } = null!;
    public bool Deleted { get; set; }
}
