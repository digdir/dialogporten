namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogLabelAssigmentLog.Queries.Search;

public class SearchDialogLabelAssignmentLogDto
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string Name { get; set; } = null!;

    public string Action { get; set; } = null!;

    public LabelAssignmentLogActorDto PerformedBy { get; set; } = null!;

    public Guid ContextId { get; set; }


}

public sealed class LabelAssignmentLogActorDto
{
    public Guid LabelAssignmentLogId { get; set; }

    public string ActorName { get; set; } = null!;

    public string ActorId { get; set; } = null!;
}
