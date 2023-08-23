using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogActivities.Queries.Get;

public sealed class GetDialogActivityDto
{
    public Guid Id { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public Uri? ExtendedType { get; set; }
    
    public DialogActivityType.Enum Type { get; set; }
    
    public DateTimeOffset? DialogDeletedAt { get; set; }
    
    public Guid? RelatedActivityId { get; set; }
    public Guid? DialogElementId { get; set; }

    public List<LocalizationDto>? PerformedBy { get; set; } = new();
    public List<LocalizationDto> Description { get; set; } = new();
}