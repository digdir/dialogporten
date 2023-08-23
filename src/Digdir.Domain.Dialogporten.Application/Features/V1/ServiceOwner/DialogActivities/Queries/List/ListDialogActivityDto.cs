using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogActivities.Queries.List;

public sealed class ListDialogActivityDto
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Uri? ExtendedType { get; set; }

    public DialogActivityType.Enum Type { get; set; }

    public DateTimeOffset? DialogDeletedAt { get; set; }

    public Guid? RelatedActivityId { get; set; }
    public Guid? DialogElementId { get; set; }
}