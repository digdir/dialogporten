using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Domain;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Attachments;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Content;
using Digdir.Domain.Dialogporten.Domain.Http;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Create;

public class CreateDialogDto
{
    public Guid? Id { get; set; }
    public string ServiceResource { get; set; } = null!;
    public string Party { get; set; } = null!;
    public int? Progress { get; set; }
    public string? ExtendedStatus { get; set; }
    public string? ExternalReference { get; set; }
    public DateTimeOffset? VisibleFrom { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }

    public DialogStatus.Values Status { get; set; }
    public List<CreateDialogContentDto> Content { get; set; } = [];

    public List<CreateDialogSearchTagDto> SearchTags { get; set; } = [];

    public List<CreateDialogDialogAttachmentDto> Attachments { get; set; } = [];
    public List<CreateDialogDialogGuiActionDto> GuiActions { get; set; } = [];
    public List<CreateDialogDialogApiActionDto> ApiActions { get; set; } = [];
    public List<CreateDialogDialogActivityDto> Activities { get; set; } = [];
}

public sealed class CreateDialogContentDto
{
    public DialogContentType.Values Type { get; set; }
    public List<LocalizationDto> Value { get; set; } = [];
    public string? MediaType { get; set; } = MediaTypes.PlainText;
}

public sealed class CreateDialogSearchTagDto
{
    public string Value { get; set; } = null!;
}

public sealed class CreateDialogDialogActivityDto
{
    public Guid? Id { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public Uri? ExtendedType { get; set; }

    public DialogActivityType.Values Type { get; set; }

    public Guid? RelatedActivityId { get; set; }

    public string? PerformedBy { get; set; }
    public List<LocalizationDto> Description { get; set; } = [];
}

public sealed class CreateDialogDialogApiActionDto
{
    public string Action { get; set; } = null!;
    public string? AuthorizationAttribute { get; set; }

    public List<CreateDialogDialogApiActionEndpointDto> Endpoints { get; set; } = [];
}

public sealed class CreateDialogDialogApiActionEndpointDto
{
    public string? Version { get; set; }
    public Uri Url { get; set; } = null!;
    public HttpVerb.Values HttpMethod { get; set; }
    public Uri? DocumentationUrl { get; set; }
    public Uri? RequestSchema { get; set; }
    public Uri? ResponseSchema { get; set; }
    public bool Deprecated { get; set; }
    public DateTimeOffset? SunsetAt { get; set; }
}

public sealed class CreateDialogDialogGuiActionDto
{
    public string Action { get; set; } = null!;
    public Uri Url { get; set; } = null!;
    public string? AuthorizationAttribute { get; set; }
    public bool IsDeleteDialogAction { get; set; }
    public HttpVerb.Values? HttpMethod { get; set; } = HttpVerb.Values.GET;

    public DialogGuiActionPriority.Values Priority { get; set; }

    public List<LocalizationDto> Title { get; set; } = [];
    public List<LocalizationDto>? Prompt { get; set; }
}

public sealed class CreateDialogDialogAttachmentDto
{
    public Guid? Id { get; set; }
    public List<LocalizationDto> DisplayName { get; set; } = [];
    public List<CreateDialogDialogAttachmentUrlDto> Urls { get; set; } = [];
}

public sealed class CreateDialogDialogAttachmentUrlDto
{
    public Uri Url { get; set; } = null!;
    public string? MediaType { get; set; } = null!;

    public DialogAttachmentUrlConsumerType.Values ConsumerType { get; set; }
}
