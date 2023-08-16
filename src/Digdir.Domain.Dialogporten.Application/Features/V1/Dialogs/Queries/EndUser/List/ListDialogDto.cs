using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Queries.EndUser.List;

public sealed class ListDialogDto
{
    public Guid Id { get; set; }
    public string Org { get; set; } = null!;
    public Uri ServiceResource { get; set; } = null!;
    public string Party { get; set; } = null!;
    public string? ExtendedStatus { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DueAt { get; set; }

    public DialogStatus.Enum Status { get; set; }

    public List<LocalizationDto> Title { get; set; } = new();
    public List<LocalizationDto>? SenderName { get; set; } 
}

public sealed class ListDialogDialogActivityDto
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Uri? ExtendedType { get; set; }

    public DialogActivityType.Enum Type { get; set; }

    public Guid? RelatedActivityId { get; set; }
    public Guid? DialogElementId { get; set; }

    public List<LocalizationDto>? PerformedBy { get; set; } = new();
    public List<LocalizationDto> Description { get; set; } = new();
}
