﻿using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Content;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actors;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Attachments;
using Digdir.Domain.Dialogporten.Domain.Http;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Get;

public sealed class GetDialogDto
{
    public Guid Id { get; set; }
    public Guid Revision { get; set; }
    public string Org { get; set; } = null!;
    public string ServiceResource { get; set; } = null!;
    public string ServiceResourceType { get; set; } = null!;
    public string Party { get; set; } = null!;
    public int? Progress { get; set; }
    public string? ExtendedStatus { get; set; }
    public string? ExternalReference { get; set; }
    public DateTimeOffset? VisibleFrom { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public DialogStatus.Values Status { get; set; }

    public GetDialogContentDto Content { get; set; } = null!;
    public string? DialogToken { get; set; }

    public List<GetDialogDialogAttachmentDto> Attachments { get; set; } = [];
    public List<GetDialogDialogGuiActionDto> GuiActions { get; set; } = [];
    public List<GetDialogDialogApiActionDto> ApiActions { get; set; } = [];
    public List<GetDialogDialogActivityDto> Activities { get; set; } = [];
    public List<GetDialogDialogSeenLogDto> SeenSinceLastUpdate { get; set; } = [];

}

public sealed class GetDialogDialogSeenLogDto
{
    public Guid Id { get; set; }
    public DateTimeOffset SeenAt { get; set; }

    public GetDialogDialogSeenLogActorDto SeenBy { get; set; } = null!;

    public bool? IsViaServiceOwner { get; set; }
    public bool IsCurrentEndUser { get; set; }
}

public sealed class GetDialogDialogSeenLogActorDto
{
    public string ActorName { get; set; } = null!;
    public string ActorId { get; set; } = null!;
}

public sealed class GetDialogContentDto
{
    public DialogContentValueDto Title { get; set; } = null!;
    public DialogContentValueDto Summary { get; set; } = null!;
    public DialogContentValueDto? SenderName { get; set; }
    public DialogContentValueDto? AdditionalInfo { get; set; }
    public DialogContentValueDto? ExtendedStatus { get; set; }
    public DialogContentValueDto? MainContentReference { get; set; }
}

public sealed class GetDialogDialogActivityDto
{
    public Guid Id { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public Uri? ExtendedType { get; set; }

    public DialogActivityType.Values Type { get; set; }

    public Guid? RelatedActivityId { get; set; }

    public GetDialogDialogActivityActorDto PerformedBy { get; set; } = null!;
    public List<LocalizationDto> Description { get; set; } = [];
}

public sealed class GetDialogDialogActivityActorDto
{
    public DialogActorType.Values ActorType { get; set; }
    public string? ActorName { get; set; }
    public string? ActorId { get; set; }
}

public sealed class GetDialogDialogApiActionDto
{
    public Guid Id { get; set; }
    public string Action { get; set; } = null!;
    public string? AuthorizationAttribute { get; set; }
    public bool IsAuthorized { get; set; }

    public List<GetDialogDialogApiActionEndpointDto> Endpoints { get; set; } = [];
}

public sealed class GetDialogDialogApiActionEndpointDto
{
    public Guid Id { get; set; }
    public string? Version { get; set; }
    public Uri Url { get; set; } = null!;
    public HttpVerb.Values HttpMethod { get; set; }
    public Uri? DocumentationUrl { get; set; }
    public Uri? RequestSchema { get; set; }
    public Uri? ResponseSchema { get; set; }
    public bool Deprecated { get; set; }
    public DateTimeOffset? SunsetAt { get; set; }
}

public sealed class GetDialogDialogGuiActionDto
{
    public Guid Id { get; set; }
    public string Action { get; set; } = null!;
    public Uri Url { get; set; } = null!;
    public string? AuthorizationAttribute { get; set; }
    public bool IsAuthorized { get; set; }
    public bool IsDeleteDialogAction { get; set; }

    public DialogGuiActionPriority.Values Priority { get; set; }
    public HttpVerb.Values HttpMethod { get; set; }

    public List<LocalizationDto> Title { get; set; } = [];
    public List<LocalizationDto>? Prompt { get; set; } = [];
}

public sealed class GetDialogDialogAttachmentDto
{
    public Guid Id { get; set; }

    public List<LocalizationDto> DisplayName { get; set; } = [];
    public List<GetDialogDialogAttachmentUrlDto> Urls { get; set; } = [];
}

public sealed class GetDialogDialogAttachmentUrlDto
{
    public Guid Id { get; set; }
    public Uri Url { get; set; } = null!;
    public string? MediaType { get; set; } = null!;

    public DialogAttachmentUrlConsumerType.Values ConsumerType { get; set; }
}
