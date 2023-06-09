using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements.DialogElementUrls;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Commands.Create;

public class CreateDialogDto
{
    public Guid? Id { get; set; }
    public string ServiceResource { get; set; } = null!;
    public string Party { get; set; } = null!;
    public DialogStatus.Enum StatusId { get; set; }
    public string? ExtendedStatus { get; set; }
    public DateTimeOffset VisibleFromUtc { get; set; }
    public DateTimeOffset? DueAtUtc { get; set; }
    public DateTimeOffset? ExpiresAtUtc { get; set; }
    public List<LocalizationDto> Body { get; set; } = new();
    public List<LocalizationDto> Title { get; set; } = new();
    public List<LocalizationDto> SenderName { get; set; } = new();
    public List<LocalizationDto> SearchTitle { get; set; } = new();
    public List<CreateDialogDialogElementDto> Elements { get; set; } = new();
    public List<CreateDialogDialogGuiActionDto> GuiActions { get; set; } = new();
    public List<CreateDialogDialogApiActionDto> ApiActions { get; set; } = new();
    public List<CreateDialogDialogActivityDto> History { get; set; } = new();
}

public sealed class CreateDialogDialogActivityDto
{
    public Guid? Id { get; set; }
    public Guid? RelatedActivityId { get; set; }
    public DateTimeOffset? CreatedAtUtc { get; set; }
    public DialogActivityType.Enum TypeId { get; set; }
    public Uri? ExtendedType { get; set; }
    public string? PerformedBy { get; set; }
    public List<LocalizationDto> Description { get; set; } = new();
    public Guid? DialogElementId { get; set; }
}

public sealed class CreateDialogDialogApiActionDto
{
    public string Action { get; set; } = null!;
    public string? AuthorizationAttribute { get; set; }
    public string HttpMethod { get; set; } = null!;
    public Uri Url { get; set; } = null!;
    public Uri? DocumentationUrl { get; set; }
    public Uri? RequestSchema { get; set; }
    public Uri? ResponseSchema { get; set; }
}

public sealed class CreateDialogDialogGuiActionDto
{
    public string Action { get; set; } = null!;
    public DialogGuiActionType.Enum TypeId { get; set; }
    public List<LocalizationDto> Title { get; set; } = new();
    public Uri Url { get; set; } = null!;
    public string? AuthorizationAttribute { get; set; }
    public bool IsBackChannel { get; set; }
    public bool IsDeleteAction { get; set; }
}

public sealed class CreateDialogDialogElementDto
{
    public Guid? Id { get; set; }
    public Guid? RelatedDialogElementId { get; set; }
    public Uri? Type { get; set; }
    public List<LocalizationDto> DisplayName { get; set; } = new();
    public string? AuthorizationAttribute { get; set; }
    public List<CreateDialogDialogElementUrlDto> Urls { get; set; } = null!;
}

public sealed class CreateDialogDialogElementUrlDto
{
    public DialogElementUrlConsumerType.Enum ConsumerTypeId { get; set; }
    public Uri Url { get; set; } = null!;
    public string? ContentTypeHint { get; set; }
}
