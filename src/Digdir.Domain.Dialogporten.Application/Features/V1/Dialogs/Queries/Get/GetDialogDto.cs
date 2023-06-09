using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements.DialogElementUrls;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Queries.Get;

public sealed class GetDialogDto
{
    public Guid Id { get; set; }
    public string Org { get; set; } = null!;
    public string ServiceResource { get; set; } = null!;
    public string Party { get; set; } = null!;
    public DialogStatus.Enum StatusId { get; set; }
    public string? ExtendedStatus { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public DateTimeOffset? ReadAtUtc { get; set; }
    public DateTimeOffset? DueAtUtc { get; set; }
    public List<LocalizationDto> Body { get; set; } = new();
    public List<LocalizationDto> Title { get; set; } = new();
    public List<LocalizationDto>? SenderName { get; set; }
    public List<LocalizationDto>? SearchTitle { get; set; }
    public List<GetDialogDialogElementDto>? Elements { get; set; }
    public List<GetDialogDialogGuiActionDto>? GuiActions { get; set; }
    public List<GetDialogDialogApiActionDto>? ApiActions { get; set; }
    public List<GetDialogDialogActivityDto> History { get; set; } = new();
    // TODO: Lenker her
}

public sealed class GetDialogDialogActivityDto
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DialogActivityType.Enum TypeId { get; set; }
    public string? PerformedBy { get; set; }
    public string? ExtendedType { get; set; }
    public List<LocalizationDto> Description { get; set; } = new();
    public Guid? DialogElementId { get; set; }
}

public sealed class GetDialogDialogApiActionDto
{
    public Guid Id { get; set; }
    public string Action { get; set; } = null!;
    public string? AuthorizationAttribute { get; set; }
    public string HttpMethod { get; set; } = null!;
    public Uri Url { get; set; } = null!;
    public Uri? DocumentationUrl { get; set; }
    public Uri? RequestSchema { get; set; }
    public Uri? ResponseSchema { get; set; }
}

public sealed class GetDialogDialogGuiActionDto
{
    public Guid Id { get; set; }
    public string Action { get; set; } = null!;
    public DialogGuiActionType.Enum TypeId { get; set; }
    public List<LocalizationDto> Title { get; set; } = new();
    public Uri Url { get; set; } = null!;
    public string? AuthorizationAttribute { get; set; }
    public bool IsBackChannel { get; set; }
    public bool IsDeleteAction { get; set; }
}

public sealed class GetDialogDialogElementDto
{
    public Guid Id { get; set; }
    public Guid? RelatedDialogElementId { get; set; }
    public Uri? Type { get; set; }
    public List<LocalizationDto> DisplayName { get; set; } = new();
    public string? AuthorizationAttribute { get; set; }
    public List<GetDialogDialogElementUrlDto> Urls { get; set; } = new();
}

public sealed class GetDialogDialogElementUrlDto
{
    public DialogElementUrlConsumerType.Enum ConsumerTypeId { get; set; }
    public Uri Url { get; set; } = null!;
    public string? ContentTypeHint { get; set; }
    public Uri? Type { get; set; }
}
