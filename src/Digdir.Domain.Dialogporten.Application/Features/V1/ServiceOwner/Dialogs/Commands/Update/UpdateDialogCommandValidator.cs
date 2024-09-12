using System.Reflection;
using Digdir.Domain.Dialogporten.Application.Common.Extensions.Enumerables;
using Digdir.Domain.Dialogporten.Application.Common.Extensions.FluentValidation;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Actors;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Content;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Domain.Actors;
using Digdir.Domain.Dialogporten.Domain.Common;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Contents;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions.Contents;
using Digdir.Domain.Dialogporten.Domain.Http;
using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Update;

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
        IValidator<UpdateDialogDialogTransmissionDto> transmissionValidator,
        IValidator<UpdateDialogDialogAttachmentDto> attachmentValidator,
        IValidator<UpdateDialogDialogGuiActionDto> guiActionValidator,
        IValidator<UpdateDialogDialogApiActionDto> apiActionValidator,
        IValidator<UpdateDialogDialogActivityDto> activityValidator,
        IValidator<UpdateDialogSearchTagDto> searchTagValidator,
        IValidator<UpdateDialogContentDto> contentValidator)
    {
        RuleFor(x => x.Progress)
            .InclusiveBetween(0, 100);

        RuleFor(x => x.ExtendedStatus)
            .MaximumLength(Constants.DefaultMaxStringLength);

        RuleFor(x => x.ExternalReference)
            .MaximumLength(Constants.DefaultMaxStringLength);

        RuleFor(x => x.ExpiresAt)
            .GreaterThanOrEqualTo(x => x.DueAt)
            .WithMessage(FluentValidationDateTimeOffsetExtensions.InFutureOfMessage)
            .When(x => x.DueAt.HasValue, ApplyConditionTo.CurrentValidator)
            .GreaterThanOrEqualTo(x => x.VisibleFrom)
            .WithMessage(FluentValidationDateTimeOffsetExtensions.InFutureOfMessage)
            .When(x => x.VisibleFrom.HasValue, ApplyConditionTo.CurrentValidator);
        RuleFor(x => x.DueAt)
            .GreaterThanOrEqualTo(x => x.VisibleFrom)
            .WithMessage(FluentValidationDateTimeOffsetExtensions.InFutureOfMessage)
            .When(x => x.VisibleFrom.HasValue, ApplyConditionTo.CurrentValidator);

        RuleFor(x => x.Status)
            .IsInEnum();

        RuleFor(x => x.Content)
            .SetValidator(contentValidator);

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
            .UniqueBy(x => x.Id)
            .ForEach(x => x.SetValidator(guiActionValidator));

        RuleFor(x => x.ApiActions)
            .UniqueBy(x => x.Id);
        RuleForEach(x => x.ApiActions)
            .SetValidator(apiActionValidator);

        RuleFor(x => x.Attachments)
            .UniqueBy(x => x.Id);
        RuleForEach(x => x.Attachments)
            .SetValidator(attachmentValidator);

        RuleFor(x => x.Transmissions)
            .UniqueBy(x => x.Id);
        RuleForEach(x => x.Transmissions)
            .SetValidator(transmissionValidator);

        RuleFor(x => x.Activities)
            .UniqueBy(x => x.Id);
        RuleForEach(x => x.Activities)
            .SetValidator(activityValidator);
    }
}

internal sealed class UpdateDialogTransmissionAttachmentDtoValidator : AbstractValidator<UpdateDialogTransmissionAttachmentDto>
{
    public UpdateDialogTransmissionAttachmentDtoValidator(
        IValidator<IEnumerable<LocalizationDto>> localizationsValidator,
        IValidator<UpdateDialogTransmissionAttachmentUrlDto> urlValidator)
    {
        RuleFor(x => x.DisplayName)
            .SetValidator(localizationsValidator);
        RuleFor(x => x.Urls)
            .NotEmpty()
            .ForEach(x => x.SetValidator(urlValidator));
    }
}

internal sealed class UpdateDialogTransmissionAttachmentUrlDtoValidator : AbstractValidator<UpdateDialogTransmissionAttachmentUrlDto>
{
    public UpdateDialogTransmissionAttachmentUrlDtoValidator()
    {
        RuleFor(x => x.Url)
            .NotNull()
            .IsValidUri()
            .MaximumLength(Constants.DefaultMaxUriLength);
        RuleFor(x => x.ConsumerType)
            .IsInEnum();
    }
}

internal sealed class UpdateDialogDialogTransmissionActorDtoValidator : AbstractValidator<UpdateDialogDialogTransmissionSenderActorDto>
{
    public UpdateDialogDialogTransmissionActorDtoValidator()
    {
        RuleFor(x => x.ActorType)
            .IsInEnum();

        RuleFor(x => x)
            .Must(dto => (dto.ActorId is null || dto.ActorName is null) &&
                         ((dto.ActorType == ActorType.Values.ServiceOwner && dto.ActorId is null && dto.ActorName is null) ||
                          (dto.ActorType != ActorType.Values.ServiceOwner && (dto.ActorId is not null || dto.ActorName is not null))))
            .WithMessage(ActorValidationErrorMessages.ActorIdActorNameExclusiveOr);

        RuleFor(x => x.ActorId!)
            .IsValidPartyIdentifier()
            .When(x => x.ActorId is not null);
    }
}

internal sealed class UpdateDialogDialogTransmissionContentDtoValidator : AbstractValidator<UpdateDialogDialogTransmissionContentDto>
{
    private static readonly Dictionary<string, PropertyInfo> SourcePropertyMetaDataByName = typeof(UpdateDialogDialogTransmissionContentDto)
        .GetProperties()
        .ToDictionary(x => x.Name, StringComparer.InvariantCultureIgnoreCase);

    public UpdateDialogDialogTransmissionContentDtoValidator()
    {
        foreach (var (propertyName, propMetadata) in SourcePropertyMetaDataByName)
        {
            RuleFor(x => propMetadata.GetValue(x) as ContentValueDto)
                .NotNull()
                .WithMessage($"{propertyName} must not be empty.")
                .SetValidator(new ContentValueDtoValidator(DialogTransmissionContentType.Parse(propertyName))!);
        }
    }
}

internal sealed class UpdateDialogDialogTransmissionDtoValidator : AbstractValidator<UpdateDialogDialogTransmissionDto>
{
    public UpdateDialogDialogTransmissionDtoValidator(
        IValidator<UpdateDialogDialogTransmissionSenderActorDto> actorValidator,
        IValidator<UpdateDialogDialogTransmissionContentDto> contentValidator,
        IValidator<UpdateDialogTransmissionAttachmentDto> attachmentValidator)
    {
        RuleFor(x => x.Id)
            .IsValidUuidV7()
            .UuidV7TimestampIsInPast();
        RuleFor(x => x.CreatedAt)
            .IsInPast();
        RuleFor(x => x.ExtendedType)
            .IsValidUri()
            .MaximumLength(Constants.DefaultMaxUriLength)
            .When(x => x.ExtendedType is not null);
        RuleFor(x => x.Type)
            .IsInEnum();
        RuleFor(x => x.RelatedTransmissionId)
            .NotEqual(x => x.Id)
            .WithMessage(x => $"A transmission cannot reference itself ({nameof(x.RelatedTransmissionId)} is equal to {nameof(x.Id)}, '{x.Id}').")
            .When(x => x.RelatedTransmissionId.HasValue);
        RuleFor(x => x.Sender)
            .NotNull()
            .SetValidator(actorValidator);
        RuleFor(x => x.AuthorizationAttribute)
            .MaximumLength(Constants.DefaultMaxStringLength);
        RuleForEach(x => x.Attachments)
            .SetValidator(attachmentValidator);
        RuleFor(x => x.Content)
            .NotEmpty()
            .SetValidator(contentValidator);
    }
}

internal sealed class UpdateDialogContentDtoValidator : AbstractValidator<UpdateDialogContentDto>
{
    private static readonly Dictionary<string, PropertyInfoWithNullability> SourcePropertyMetaDataByName =
        typeof(UpdateDialogContentDto).GetProperties()
            .Select(x =>
            {
                var context = new NullabilityInfoContext();
                var nullabilityInfo = context.Create(x);

                return new PropertyInfoWithNullability(x, nullabilityInfo);
            })
            .ToDictionary(x => x.Property.Name, StringComparer.InvariantCultureIgnoreCase);

    public UpdateDialogContentDtoValidator(IUser? user)
    {
        foreach (var (propertyName, propMetadata) in SourcePropertyMetaDataByName)
        {
            switch (propMetadata.NullabilityInfo.WriteState)
            {
                case NullabilityState.NotNull:
                    RuleFor(x => propMetadata.Property.GetValue(x) as ContentValueDto)
                        .NotNull()
                        .WithMessage($"{propertyName} must not be empty.")
                        .SetValidator(
                            new ContentValueDtoValidator(DialogContentType.Parse(propertyName), user)!);
                    break;
                case NullabilityState.Nullable:
                    RuleFor(x => propMetadata.Property.GetValue(x) as ContentValueDto)
                        .SetValidator(
                            new ContentValueDtoValidator(DialogContentType.Parse(propertyName), user)!)
                        .When(x => propMetadata.Property.GetValue(x) is not null);
                    break;
                case NullabilityState.Unknown:
                    break;
                default:
                    break;
            }
        }
    }
}

internal sealed class UpdateDialogDialogAttachmentDtoValidator : AbstractValidator<UpdateDialogDialogAttachmentDto>
{
    public UpdateDialogDialogAttachmentDtoValidator(
        IValidator<IEnumerable<LocalizationDto>> localizationsValidator,
        IValidator<UpdateDialogDialogAttachmentUrlDto> urlValidator)
    {
        RuleFor(x => x.Id)
            .IsValidUuidV7()
            .UuidV7TimestampIsInPast();
        RuleFor(x => x.DisplayName)
            .SetValidator(localizationsValidator);
        RuleFor(x => x.Urls)
            .UniqueBy(x => x.Id);
        RuleFor(x => x.Urls)
            .NotEmpty()
            .ForEach(x => x.SetValidator(urlValidator));
    }
}

internal sealed class UpdateDialogDialogAttachmentUrlDtoValidator : AbstractValidator<UpdateDialogDialogAttachmentUrlDto>
{
    public UpdateDialogDialogAttachmentUrlDtoValidator()
    {
        RuleFor(x => x.Url)
            .NotNull()
            .IsValidUri()
            .MaximumLength(Constants.DefaultMaxUriLength);
        RuleFor(x => x.ConsumerType)
            .IsInEnum();
    }
}

internal sealed class UpdateDialogSearchTagDtoValidator : AbstractValidator<UpdateDialogSearchTagDto>
{
    public UpdateDialogSearchTagDtoValidator()
    {
        RuleFor(x => x.Value)
            .MinimumLength(3)
            .MaximumLength(Constants.MaxSearchTagLength);
    }
}

internal sealed class UpdateDialogDialogGuiActionDtoValidator : AbstractValidator<UpdateDialogDialogGuiActionDto>
{
    public UpdateDialogDialogGuiActionDtoValidator(
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
    }
}

internal sealed class UpdateDialogDialogActivityDtoValidator : AbstractValidator<UpdateDialogDialogActivityDto>
{
    public UpdateDialogDialogActivityDtoValidator(
        IValidator<IEnumerable<LocalizationDto>> localizationsValidator,
        IValidator<UpdateDialogDialogActivityPerformedByActorDto> actorValidator)
    {
        RuleFor(x => x.Id)
            .IsValidUuidV7()
            .UuidV7TimestampIsInPast();
        RuleFor(x => x.CreatedAt)
            .IsInPast();
        RuleFor(x => x.ExtendedType)
            .IsValidUri()
            .MaximumLength(Constants.DefaultMaxUriLength);
        RuleFor(x => x.Type)
            .IsInEnum();
        RuleFor(x => x.RelatedActivityId)
            .NotEqual(x => x.Id)
            .WithMessage(x => $"An activity cannot reference itself ({nameof(x.RelatedActivityId)} is equal to {nameof(x.Id)}, '{x.Id}').")
            .When(x => x.RelatedActivityId.HasValue);
        RuleFor(x => x.PerformedBy)
            .NotNull()
            .SetValidator(actorValidator);
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required when the type is '" + nameof(DialogActivityType.Values.Information) + "'.")
            .SetValidator(localizationsValidator)
            .When(x => x.Type == DialogActivityType.Values.Information);
        RuleFor(x => x.Description)
            .Empty()
            .WithMessage("Description is only allowed when the type is '" + nameof(DialogActivityType.Values.Information) + "'.")
            .When(x => x.Type != DialogActivityType.Values.Information);
        RuleFor(x => x.TransmissionId)
            .Null()
            .WithMessage($"A {nameof(DialogActivityType.Values.DialogOpened)} activity cannot reference a transmission.")
            .When(x => x.Type == DialogActivityType.Values.DialogOpened);
        RuleFor(x => x.TransmissionId)
            .NotEmpty()
            .WithMessage($"A {nameof(DialogActivityType.Values.TransmissionOpened)} needs to reference a transmission.")
            .When(x => x.Type == DialogActivityType.Values.TransmissionOpened);
    }
}

internal sealed class UpdateDialogDialogActivityActorDtoValidator : AbstractValidator<UpdateDialogDialogActivityPerformedByActorDto>
{
    public UpdateDialogDialogActivityActorDtoValidator()
    {
        RuleFor(x => x.ActorType)
            .IsInEnum();

        RuleFor(x => x)
            .Must(dto => (dto.ActorId is null || dto.ActorName is null) &&
                         ((dto.ActorType == ActorType.Values.ServiceOwner && dto.ActorId is null && dto.ActorName is null) ||
                          (dto.ActorType != ActorType.Values.ServiceOwner && (dto.ActorId is not null || dto.ActorName is not null))))
            .WithMessage(ActorValidationErrorMessages.ActorIdActorNameExclusiveOr);

        RuleFor(x => x.ActorId!)
            .IsValidPartyIdentifier()
            .When(x => x.ActorId is not null);
    }
}
