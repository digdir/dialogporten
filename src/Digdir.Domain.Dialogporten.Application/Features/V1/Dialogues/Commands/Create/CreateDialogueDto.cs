﻿using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Domain.Dialogues;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Activities;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Commands.Create;

public class CreateDialogueDto
{
    public Guid Id { get; set; }
    public string ServiceResourceIdentifier { get; set; } = null!;
    public string Party { get; set; } = null!;
    public DialogueStatus.Enum StatusId { get; set; }
    public string? ExtendedStatus { get; set; }
    public DateTime VisibleFromUtc { get; set; }
    public DateTime? DueAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public List<LocalizationDto> Body { get; set; } = new();
    public List<LocalizationDto> Title { get; set; } = new();
    public List<LocalizationDto> SenderName { get; set; } = new();
    public List<LocalizationDto> SearchTitle { get; set; } = new();
    public List<CreateDialogueDialogueAttachmentDto> Attachments { get; set; } = new();
    public List<CreateDialogueDialogueGuiActionDto> GuiActions { get; set; } = new();
    public List<CreateDialogueDialogueApiActionDto> ApiActions { get; set; } = new();
    public List<CreateDialogueDialogueActivityDto> History { get; set; } = new();
    public List<CreateDialogueDialogueTokenScopeDto> TokenScopes { get; set; } = new();
}

public sealed class CreateDialogueDialogueTokenScopeDto
{
    public string Value { get; set; } = null!;
}

public sealed class CreateDialogueDialogueActivityDto
{
    public Guid? Id { get; set; }
    public DateTime? CreatedAtUtc { get; set; }
    public DialogueActivityType.Enum TypeId { get; set; }
    public string? PerformedBy { get; set; }
    public string? ExtendedType { get; set; }
    public List<LocalizationDto> Description { get; set; } = new();
    public Uri? DetailsApiUrl { get; set; }
    public Uri? DetailsGuiUrl { get; set; }
}

public sealed class CreateDialogueDialogueApiActionDto
{
    public string Action { get; set; } = null!;
    public string HttpMethod { get; set; } = null!;
    public Uri Url { get; set; } = null!;
    public Uri? DocumentationUrl { get; set; }
    public Uri? RequestSchema { get; set; }
    public Uri? ResponseSchema { get; set; }
}

public sealed class CreateDialogueDialogueGuiActionDto
{
    public string Action { get; set; } = null!;
    public DialogueGuiActionType.Enum TypeId { get; set; }
    public List<LocalizationDto> Title { get; set; } = new();
    public Uri Url { get; set; } = null!;
    public string? Resource { get; set; }
    public bool IsBackChannel { get; set; }
    public bool IsDeleteAction { get; set; }
}

public sealed class CreateDialogueDialogueAttachmentDto
{
    public List<LocalizationDto> DisplayName { get; set; } = new();
    public long SizeInBytes { get; set; }
    public string ContentType { get; set; } = null!;
    public Uri Url { get; set; } = null!;
    public string? Resource { get; set; }
}