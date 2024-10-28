namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogLabelAssignmentLog.Queries.Search;

public sealed class DialogLabelAssignmentLogDto
{
    public DateTimeOffset CreatedAt { get; set; }

    public string Name { get; set; } = null!;

    public string Action { get; set; } = null!;

    public LabelAssignmentLogActorDto PerformedBy { get; set; } = null!;

}

public sealed class LabelAssignmentLogActorDto
{

    public string ActorName { get; set; } = null!;

    public string ActorId { get; set; } = null!;
}
