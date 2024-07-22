using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actors;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Content;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Search;

public sealed class SearchDialogDto
{
    public Guid Id { get; set; }
    public string Org { get; set; } = null!;
    public string ServiceResource { get; set; } = null!;
    public string ServiceResourceType { get; set; } = null!;
    public string Party { get; set; } = null!;
    public int? Progress { get; set; }
    public int? GuiAttachmentCount { get; set; }
    public string? ExtendedStatus { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DueAt { get; set; }

    public DialogStatus.Values Status { get; set; }

    public SearchDialogDialogActivityDto? LatestActivity { get; set; }

    public List<SearchDialogContentDto> Content { get; set; } = [];
    public List<SearchDialogDialogSeenLogDto> SeenSinceLastUpdate { get; set; } = [];
}

public sealed class SearchDialogDialogSeenLogDto
{
    public Guid Id { get; set; }
    public DateTimeOffset SeenAt { get; set; }

    public SearchDialogDialogSeenLogActorDto SeenBy { get; set; } = null!;

    public bool? IsViaServiceOwner { get; set; }
    public bool IsCurrentEndUser { get; set; }
}

public sealed class SearchDialogDialogSeenLogActorDto
{
    public string ActorName { get; set; } = null!;
    public string ActorId { get; set; } = null!;
}

public sealed class SearchDialogContentDto
{
    public DialogContentType.Values Type { get; set; }
    public List<LocalizationDto> Value { get; set; } = [];
}

public sealed class SearchDialogDialogActivityDto
{
    public Guid Id { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public Uri? ExtendedType { get; set; }

    public DialogActivityType.Values Type { get; set; }

    public Guid? RelatedActivityId { get; set; }

    public SearchDialogDialogActivityActorDto PerformedBy { get; set; } = null!;
    public List<LocalizationDto> Description { get; set; } = [];
}

public sealed class SearchDialogDialogActivityActorDto
{
    public DialogActorType.Values ActorType { get; set; }
    public string? ActorName { get; set; }
    public string? ActorId { get; set; }
}
