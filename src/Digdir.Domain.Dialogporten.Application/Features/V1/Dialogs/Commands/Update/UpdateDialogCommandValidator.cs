using Digdir.Domain.Dialogporten.Application.Common.Extensions.FluentValidation;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Domain.Common;
using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Commands.Update;

internal sealed class UpdateDialogCommandValidator : AbstractValidator<UpdateDialogCommand>
{
    public UpdateDialogCommandValidator(IValidator<UpdateDialogDto> updateDialogDtoValidator)
    {
        RuleFor(x => x.Id)
            .NotEmpty();
        RuleFor(x => x.Dto)
            .NotEmpty()
            .SetValidator(updateDialogDtoValidator);
    }
}

internal sealed class UpdateDialogDtoValidator : AbstractValidator<UpdateDialogDto>
{
    public UpdateDialogDtoValidator(
        IValidator<IEnumerable<LocalizationDto>> localizatiosnValidator,
        IValidator<UpdateDialogDialogElementDto> elementValidator,
        IValidator<UpdateDialogDialogGuiActionDto> guiActionValidator,
        IValidator<UpdateDialogDialogApiActionDto> apiActionValidator,
        IValidator<UpdateDialogDialogActivityDto> activityValidator)
    {
        RuleFor(x => x.ExtendedStatus)
            .MaximumLength(Constants.DefaultMaxStringLength);

        RuleFor(x => x.ExpiresAt)
            .IsUtcKind()
            .IsInFuture()
            .GreaterThanOrEqualTo(x => x.DueAt)
                .WithMessage(FluentValidation_DateTime_Extensions.InFutureOfMessage)
                .When(x => x.DueAt.HasValue, ApplyConditionTo.CurrentValidator)
            .GreaterThanOrEqualTo(x => x.VisibleFrom)
                .WithMessage(FluentValidation_DateTime_Extensions.InFutureOfMessage)
                .When(x => x.VisibleFrom.HasValue, ApplyConditionTo.CurrentValidator);
        RuleFor(x => x.DueAt)
            .IsUtcKind()
            .IsInFuture()
            .GreaterThanOrEqualTo(x => x.VisibleFrom)
                .WithMessage(FluentValidation_DateTime_Extensions.InFutureOfMessage)
                .When(x => x.VisibleFrom.HasValue, ApplyConditionTo.CurrentValidator);
        RuleFor(x => x.VisibleFrom)
            .IsUtcKind()
            .IsInFuture();

        RuleFor(x => x.Status)
            .IsInEnum();

        RuleFor(x => x.Title)
            .NotEmpty()
            .SetValidator(localizatiosnValidator);
        RuleFor(x => x.Body)
            .SetValidator(new LocalizationDtosValidator(maximumLength: 1023));
        RuleForEach(x => x.Body)
            .ContainsValidHttp();
        RuleFor(x => x.SenderName)
            .SetValidator(localizatiosnValidator);
        RuleFor(x => x.SearchTitle)
            .SetValidator(localizatiosnValidator);

        RuleFor(x => x.GuiActions)
            .UniqueBy(x => x.Id);
        RuleForEach(x => x.GuiActions)
            .SetValidator(guiActionValidator);

        RuleFor(x => x.ApiActions)
            .UniqueBy(x => x.Id);
        RuleForEach(x => x.ApiActions)
            .IsIn(x => x.Elements,
                dependentKeySelector: action => action.DialogElementId,
                principalKeySelector: element => element.Id)
            .SetValidator(apiActionValidator);

        RuleFor(x => x.Elements)
            .UniqueBy(x => x.Id);
        RuleForEach(x => x.Elements)
            .IsIn(x => x.Elements,
                dependentKeySelector: element => element.RelatedDialogElementId,
                principalKeySelector: element => element.Id)
            .SetValidator(elementValidator);

        RuleFor(x => x.Activities)
            .UniqueBy(x => x.Id);
        RuleForEach(x => x.Activities)
            .IsIn(x => x.Elements,
                dependentKeySelector: activity => activity.DialogElementId,
                principalKeySelector: element => element.Id)
            .IsIn(x => x.Activities,
                dependentKeySelector: activity => activity.RelatedActivityId,
                principalKeySelector: activity => activity.Id)
            .SetValidator(activityValidator);
    }
}

internal sealed class UpdateDialogDialogElementDtoValidator : AbstractValidator<UpdateDialogDialogElementDto>
{
    public UpdateDialogDialogElementDtoValidator(
        IValidator<IEnumerable<LocalizationDto>> localizatiosnValidator,
        IValidator<UpdateDialogDialogElementUrlDto> urlValidator)
    {
        RuleFor(x => x.Id)
            .NotEqual(default(Guid))
            .IsValidUuidV7();
        RuleFor(x => x.Type)
            .IsValidUri()
            .MaximumLength(Constants.DefaultMaxUriLength);
        RuleFor(x => x.AuthorizationAttribute)
            .MaximumLength(Constants.DefaultMaxStringLength);
        RuleFor(x => x.RelatedDialogElementId)
            .NotEqual(x => x.Id)
            .When(x => x.RelatedDialogElementId.HasValue);
        RuleFor(x => x.DisplayName)
            .SetValidator(localizatiosnValidator);
        RuleFor(x => x.Urls)
            .UniqueBy(x => x.Id);
        RuleFor(x => x.Urls)
            .NotEmpty()
            .ForEach(x => x.SetValidator(urlValidator));
    }
}

internal sealed class UpdateDialogDialogElementUrlDtoValidator : AbstractValidator<UpdateDialogDialogElementUrlDto>
{
    public UpdateDialogDialogElementUrlDtoValidator()
    {
        RuleFor(x => x.Url)
            .NotNull()
            .IsValidUri()
            .MaximumLength(Constants.DefaultMaxUriLength);
        RuleFor(x => x.MimeType)
            .MaximumLength(Constants.DefaultMaxStringLength);
        RuleFor(x => x.ConsumerType)
            .IsInEnum();
    }
}

internal sealed class UpdateDialogDialogGuiActionDtoValidator : AbstractValidator<UpdateDialogDialogGuiActionDto>
{
    public UpdateDialogDialogGuiActionDtoValidator(
        IValidator<IEnumerable<LocalizationDto>> localizatiosnValidator)
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
        RuleFor(x => x.Title)
            .NotEmpty()
            .SetValidator(localizatiosnValidator);
    }
}

internal sealed class UpdateDialogDialogApiActionDtoValidator : AbstractValidator<UpdateDialogDialogApiActionDto>
{
    public UpdateDialogDialogApiActionDtoValidator(
        IValidator<UpdateDialogDialogApiActionEndpointDto> apiActionEndpointValidator)
    {
        RuleFor(x => x.Action)
            .NotEmpty()
            .MaximumLength(Constants.DefaultMaxStringLength);
        RuleFor(x => x.AuthorizationAttribute)
            .MaximumLength(Constants.DefaultMaxStringLength);
        RuleFor(x => x.Endpoints)
            .UniqueBy(x => x.Id);
        RuleFor(x => x.Endpoints)
            .NotEmpty()
            .ForEach(x => x.SetValidator(apiActionEndpointValidator));
    }
}

internal sealed class UpdateDialogDialogApiActionEndpointDtoValidator : AbstractValidator<UpdateDialogDialogApiActionEndpointDto>
{
    public UpdateDialogDialogApiActionEndpointDtoValidator()
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
        RuleFor(x => x.SunsetAt)
            .IsUtcKind();
    }
}

internal sealed class UpdateDialogDialogActivityDtoValidator : AbstractValidator<UpdateDialogDialogActivityDto>
{
    public UpdateDialogDialogActivityDtoValidator(
        IValidator<IEnumerable<LocalizationDto>> localizatiosnValidator)
    {
        RuleFor(x => x.Id)
            .NotEqual(default(Guid))
            .IsValidUuidV7();
        RuleFor(x => x.CreatedAt)
            .IsUtcKind()
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
            .SetValidator(localizatiosnValidator);
        RuleFor(x => x.Description)
            .NotEmpty()
            .SetValidator(localizatiosnValidator);
    }
}