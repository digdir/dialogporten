﻿using Digdir.Domain.Dialogporten.Application.Common.Extensions.Enumerables;
using Digdir.Domain.Dialogporten.Application.Common.Extensions.FluentValidation;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Domain;
using Digdir.Domain.Dialogporten.Domain.Common;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Content;
using Digdir.Domain.Dialogporten.Domain.Http;
using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Create;

internal sealed class CreateDialogCommandValidator : AbstractValidator<CreateDialogCommand>
{
    public CreateDialogCommandValidator(
        IValidator<CreateDialogDialogAttachmentDto> attachmentValidator,
        IValidator<CreateDialogDialogGuiActionDto> guiActionValidator,
        IValidator<CreateDialogDialogApiActionDto> apiActionValidator,
        IValidator<CreateDialogDialogActivityDto> activityValidator,
        IValidator<CreateDialogSearchTagDto> searchTagValidator,
        IValidator<CreateDialogContentDto> contentValidator)
    {
        RuleFor(x => x.Id)
            .NotEqual(default(Guid));

        RuleFor(x => x.ServiceResource)
            .NotNull()
            .IsValidUri()
            .MaximumLength(Constants.DefaultMaxUriLength)
            .Must(x =>
                x?.StartsWith(Constants.ServiceResourcePrefix, StringComparison.InvariantCulture) ?? false)
                .WithMessage($"'{{PropertyName}}' must start with '{Constants.ServiceResourcePrefix}'.");

        RuleFor(x => x.Party)
            .IsValidPartyIdentifier()
            .NotEmpty()
            .MaximumLength(Constants.DefaultMaxStringLength);

        RuleFor(x => x.Progress)
            .InclusiveBetween(0, 100);

        RuleFor(x => x.ExtendedStatus)
            .MaximumLength(Constants.DefaultMaxStringLength);

        RuleFor(x => x.ExternalReference)
            .MaximumLength(Constants.DefaultMaxStringLength);

        RuleFor(x => x.ExpiresAt)
            .IsInFuture()
            .GreaterThanOrEqualTo(x => x.DueAt)
                .WithMessage(FluentValidationDateTimeOffsetExtensions.InFutureOfMessage)
                .When(x => x.DueAt.HasValue, ApplyConditionTo.CurrentValidator)
            .GreaterThanOrEqualTo(x => x.VisibleFrom)
                .WithMessage(FluentValidationDateTimeOffsetExtensions.InFutureOfMessage)
                .When(x => x.VisibleFrom.HasValue, ApplyConditionTo.CurrentValidator);
        RuleFor(x => x.DueAt)
            .IsInFuture()
            .GreaterThanOrEqualTo(x => x.VisibleFrom)
                .WithMessage(FluentValidationDateTimeOffsetExtensions.InFutureOfMessage)
                .When(x => x.VisibleFrom.HasValue, ApplyConditionTo.CurrentValidator);
        RuleFor(x => x.VisibleFrom)
            .IsInFuture();

        RuleFor(x => x.Status)
            .IsInEnum();

        RuleFor(x => x.Content)
            .UniqueBy(x => x.Type)
            .Must(content => DialogContentType.RequiredTypes
                .All(requiredContent => content
                    .EmptyIfNull()
                    .Select(x => x.Type)
                    .Contains(requiredContent)))
            .WithMessage("Dialog must contain the following content: " +
                         $"[{string.Join(", ", DialogContentType.RequiredTypes)}].")
            .ForEach(x => x.SetValidator(contentValidator));

        RuleFor(x => x.SearchTags)
            .UniqueBy(x => x.Value, StringComparer.InvariantCultureIgnoreCase)
            .ForEach(x => x.SetValidator(searchTagValidator));

        RuleFor(x => x.GuiActions)
            .Must(x => x
                .EmptyIfNull()
                .Count(x => x.Priority == DialogGuiActionPriority.Values.Primary) <= 1)
                .WithMessage("Only one primary GUI action is allowed.")
            .Must(x => x
                .EmptyIfNull()
                .Count(x => x.Priority == DialogGuiActionPriority.Values.Secondary) <= 1)
                .WithMessage("Only one secondary GUI action is allowed.")
            .Must(x => x
                .EmptyIfNull()
                .Count(x => x.Priority == DialogGuiActionPriority.Values.Tertiary) <= 5)
                .WithMessage("Only five tertiary GUI actions are allowed.")
            .ForEach(x => x.SetValidator(guiActionValidator));

        RuleForEach(x => x.ApiActions)
            .SetValidator(apiActionValidator);

        RuleFor(x => x.Attachments)
            .UniqueBy(x => x.Id);
        RuleForEach(x => x.Attachments)
            .SetValidator(attachmentValidator);

        RuleFor(x => x.Activities)
            .UniqueBy(x => x.Id);
        RuleForEach(x => x.Activities)
            .IsIn(x => x.Activities,
                dependentKeySelector: activity => activity.RelatedActivityId,
                principalKeySelector: activity => activity.Id)
            .SetValidator(activityValidator);
    }
}

internal sealed class CreateDialogContentDtoValidator : AbstractValidator<CreateDialogContentDto>
{
    public CreateDialogContentDtoValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.Type)
            .IsInEnum();
        RuleFor(x => x.MediaType)
            .Must((dto, value) =>
            {
                var type = DialogContentType.GetValue(dto.Type);
                return value is null ? type.AllowedMediaTypes.Length == 0 : type.AllowedMediaTypes.Contains(value);
            })
            .WithMessage(x =>
                $"{{PropertyName}} '{x.MediaType ?? "null"}' is not allowed for content type {DialogContentType.GetValue(x.Type).Name}. " +
                $"Valid media types are: {(DialogContentType.GetValue(x.Type).AllowedMediaTypes.Length == 0 ? "None" :
                    $"{string.Join(", ", DialogContentType.GetValue(x.Type).AllowedMediaTypes!)}")}");
        RuleForEach(x => x.Value)
            .ContainsValidHtml()
            .When(x => x.MediaType is not null && (x.MediaType == MediaTypes.Html));
        RuleForEach(x => x.Value)
            .ContainsValidMarkdown()
            .When(x => x.MediaType is not null && x.MediaType == MediaTypes.Markdown);
        RuleForEach(x => x.Value)
            .Must(x => Uri.TryCreate(x.Value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps)
            .When(x => x.MediaType is not null && x.MediaType.StartsWith(MediaTypes.EmbeddablePrefix, StringComparison.InvariantCultureIgnoreCase))
            .WithMessage("{PropertyName} must be a valid HTTPS URL for embeddable content types");
        RuleFor(x => x.Value)
            .NotEmpty()
            .SetValidator(x => new LocalizationDtosValidator(DialogContentType.GetValue(x.Type).MaxLength));
    }
}

internal sealed class CreateDialogDialogAttachmentDtoValidator : AbstractValidator<CreateDialogDialogAttachmentDto>
{
    public CreateDialogDialogAttachmentDtoValidator(
        IValidator<IEnumerable<LocalizationDto>> localizationsValidator,
        IValidator<CreateDialogDialogAttachmentUrlDto> urlValidator)
    {
        RuleFor(x => x.Id)
            .NotEqual(default(Guid));
        RuleFor(x => x.DisplayName)
            .SetValidator(localizationsValidator);
        RuleFor(x => x.Urls)
            .NotEmpty()
            .ForEach(x => x.SetValidator(urlValidator));
    }
}

internal sealed class CreateDialogDialogAttachmentUrlDtoValidator : AbstractValidator<CreateDialogDialogAttachmentUrlDto>
{
    public CreateDialogDialogAttachmentUrlDtoValidator()
    {
        RuleFor(x => x.Url)
            .NotNull()
            .IsValidUri()
            .MaximumLength(Constants.DefaultMaxUriLength);
        RuleFor(x => x.ConsumerType)
            .IsInEnum();
    }
}

internal sealed class CreateDialogSearchTagDtoValidator : AbstractValidator<CreateDialogSearchTagDto>
{
    public CreateDialogSearchTagDtoValidator()
    {
        RuleFor(x => x.Value)
            .MinimumLength(3)
            .MaximumLength(Constants.MaxSearchTagLength);
    }
}

internal sealed class CreateDialogDialogGuiActionDtoValidator : AbstractValidator<CreateDialogDialogGuiActionDto>
{
    public CreateDialogDialogGuiActionDtoValidator(
        IValidator<IEnumerable<LocalizationDto>> localizationsValidator)
    {
        RuleFor(x => x.Action)
            .NotEmpty()
            .MaximumLength(Constants.DefaultMaxStringLength);
        RuleFor(x => x.Url)
            .NotNull()
            .IsValidUri()
            .MaximumLength(Constants.DefaultMaxUriLength);
        RuleFor(x => x.AuthorizationAttribute)
            .MaximumLength(Constants.DefaultMaxStringLength);
        RuleFor(x => x.Priority)
            .IsInEnum();
        RuleFor(x => x.HttpMethod)
            .Must(x => x is HttpVerb.Values.GET or HttpVerb.Values.POST or HttpVerb.Values.DELETE)
            .WithMessage($"'{{PropertyName}}' for GUI actions must be one of the following: " +
                         $"[{HttpVerb.Values.GET}, {HttpVerb.Values.POST}, {HttpVerb.Values.DELETE}].");
        RuleFor(x => x.Title)
            .NotEmpty()
            .SetValidator(localizationsValidator);
        RuleFor(x => x.Prompt)
            .SetValidator(localizationsValidator!)
            .When(x => x.Prompt != null);
    }
}

internal sealed class CreateDialogDialogApiActionDtoValidator : AbstractValidator<CreateDialogDialogApiActionDto>
{
    public CreateDialogDialogApiActionDtoValidator(
        IValidator<CreateDialogDialogApiActionEndpointDto> apiActionEndpointValidator)
    {
        RuleFor(x => x.Action)
            .NotEmpty()
            .MaximumLength(Constants.DefaultMaxStringLength);
        RuleFor(x => x.AuthorizationAttribute)
            .MaximumLength(Constants.DefaultMaxStringLength);
        RuleFor(x => x.Endpoints)
            .NotEmpty()
            .ForEach(x => x.SetValidator(apiActionEndpointValidator));
    }
}

internal sealed class CreateDialogDialogApiActionEndpointDtoValidator : AbstractValidator<CreateDialogDialogApiActionEndpointDto>
{
    public CreateDialogDialogApiActionEndpointDtoValidator()
    {
        RuleFor(x => x.Version)
            .MaximumLength(Constants.DefaultMaxStringLength);
        RuleFor(x => x.Url)
            .NotNull()
            .IsValidUri()
            .MaximumLength(Constants.DefaultMaxUriLength);
        RuleFor(x => x.HttpMethod)
            .IsInEnum();
        RuleFor(x => x.DocumentationUrl)
            .IsValidUri()
            .MaximumLength(Constants.DefaultMaxUriLength);
        RuleFor(x => x.RequestSchema)
            .IsValidUri()
            .MaximumLength(Constants.DefaultMaxUriLength);
        RuleFor(x => x.ResponseSchema)
            .IsValidUri()
            .MaximumLength(Constants.DefaultMaxUriLength);
        RuleFor(x => x.Deprecated)
            .Equal(true)
            .When(x => x.SunsetAt.HasValue);
    }
}

internal sealed class CreateDialogDialogActivityDtoValidator : AbstractValidator<CreateDialogDialogActivityDto>
{
    public CreateDialogDialogActivityDtoValidator(
        IValidator<IEnumerable<LocalizationDto>> localizationsValidator)
    {
        RuleFor(x => x.Id)
            .NotEqual(default(Guid));
        RuleFor(x => x.CreatedAt)
            .IsInPast();
        RuleFor(x => x.ExtendedType)
            .IsValidUri()
            .MaximumLength(Constants.DefaultMaxUriLength);
        RuleFor(x => x.Type)
            .IsInEnum();
        RuleFor(x => x.RelatedActivityId)
            .NotEqual(x => x.Id)
            .When(x => x.RelatedActivityId.HasValue);
        RuleFor(x => x.PerformedBy)
            .MaximumLength(Constants.DefaultMaxStringLength);
        RuleFor(x => x.Description)
            .NotEmpty()
            .SetValidator(localizationsValidator);
    }
}
