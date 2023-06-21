using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Domain.Common;
using FluentValidation;
using System.Diagnostics.CodeAnalysis;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Commands.Create;

internal static class NumberHelper
{
    private static readonly int[] _orgNumberFactors = new[] { 3, 2, 7, 6, 5, 4, 3, 2 };
    private static readonly int[] _personNumberFactors1 = new[] { 3, 7, 6, 1, 8, 9, 4, 5, 2, 1 };
    private static readonly int[] _personNumberFactors2 = new[] { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2, 1 };
    private const int Mod11 = 11;
    
    public static bool IsValidOrgNumber(string number)
    {
        return number.Length == 9
            && TryCalculateControlDigitMod11(number[0..9], _orgNumberFactors, out var control)
            && control == int.Parse(number[9..10]);
    }

    public static bool IsValidPersonNumber(string number)
    {
        return number.Length == 11
            && TryCalculateControlDigitMod11(number[..9], _personNumberFactors1, out var control1)
            && TryCalculateControlDigitMod11(number[..10], _personNumberFactors2, out var control2)
            && control1 == int.Parse(number[9..10])
            && control2 == int.Parse(number[10..11]);
    }

    private static bool TryCalculateControlDigitMod11(string nummer, int[] factors, [NotNullWhen(true)] out int? controlDigit)
    {
        controlDigit = null;
        var numberConverts = nummer
            .Select(x => (Success: int.TryParse(x.ToString(), out var n), Number: n))
            .ToArray();

        if (numberConverts.Any(x => !x.Success))
        {
            return false;
        }

        var sum = numberConverts
            .Select(x => x.Number)
            .Select((digit, index) => digit * factors[index])
            .Sum();
        controlDigit = Mod11 - (sum % Mod11);
        return true;
    }
}

internal sealed class CreateDialogCommandValidator : AbstractValidator<CreateDialogCommand>
{
    public CreateDialogCommandValidator(
        IValidator<LocalizationDto> localizationValidator,
        IValidator<CreateDialogDialogElementDto> elementValidator,
        IValidator<CreateDialogDialogGuiActionDto> guiActionValidator,
        IValidator<CreateDialogDialogApiActionDto> apiActionValidator,
        IValidator<CreateDialogDialogActivityDto> activityValidator)
    {
        // TODO: Valider at ID er på uuid v7 format dersom det er satt
        RuleFor(x => x.Id).NotEqual(default(Guid));

        RuleFor(x => x.ServiceResource)
            .NotNull()
            .IsValidUri()
            .MaximumLength(Constants.DefaultMaxUriLength)
            .Must(x => x.ToString().StartsWith("urn:altinn:resource:"))
                .WithMessage("'{PropertyName}' must start with 'urn:altinn:resource:'.");

        // TODO: Party skal starte med enten /org/ eller /person/ og inneholde hhv et organisasjonsnummer eller
        // et f-eller d-nummer. Kun sjekk well-formed-ness (mod 11-sjekk for orgnr og f/dnr)
        // DU ER HER! Er D-nummer validering lik personnummer validering? Skriv en utfyldende beskjed dersom validering feiler. 
        RuleFor(x => x.Party)
            .Must(x => x.Split('/') switch
            {
                ["", "org", var orgNumberAsString] => NumberHelper.IsValidOrgNumber(orgNumberAsString),
                ["", "person", var personEllerDNummer] => NumberHelper.IsValidPersonNumber(personEllerDNummer),
                _ => false
            }).WithMessage("'{PropertyName}'")
            .NotEmpty()
            .MaximumLength(Constants.DefaultMaxStringLength);

        RuleFor(x => x.ExtendedStatus)
            .MaximumLength(Constants.DefaultMaxStringLength);

        RuleFor(x => x.ExpiresAt)
            .GreaterThanOrEqualTo(DateTimeOffset.UtcNow)
            .GreaterThanOrEqualTo(x => x.DueAt)
            .GreaterThanOrEqualTo(x => x.VisibleFrom);
        RuleFor(x => x.DueAt)
            .GreaterThanOrEqualTo(DateTimeOffset.UtcNow)
            .GreaterThanOrEqualTo(x => x.VisibleFrom);
        RuleFor(x => x.VisibleFrom)
            .GreaterThanOrEqualTo(DateTimeOffset.UtcNow);

        RuleFor(x => x.StatusId).IsInEnum();

        RuleFor(x => x.Title)
            .NotEmpty()
            .ForEach(x => x.SetValidator(localizationValidator));
        // TODO: Valider iht https://github.com/orgs/digdir/projects/7/views/1?pane=issue&itemId=30057377
        RuleForEach(x => x.Body).SetValidator(new LocalizationDtoValidator(maximumLength: 1023));
        RuleForEach(x => x.SenderName).SetValidator(localizationValidator);
        RuleForEach(x => x.SearchTitle).SetValidator(localizationValidator);

        RuleForEach(x => x.GuiActions).SetValidator(guiActionValidator);

        RuleForEach(x => x.ApiActions)
            .IsIn(x => x.Elements,
                dependentKeySelector: action => action.DialogElementId,
                principalKeySelector: element => element.Id)
            .SetValidator(apiActionValidator);

        RuleForEach(x => x.Elements)
            .IsIn(x => x.Elements,
                dependentKeySelector: element => element.RelatedDialogElementId,
                principalKeySelector: element => element.Id)
            .SetValidator(elementValidator);

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

internal sealed class CreateDialogDialogElementDtoValidator : AbstractValidator<CreateDialogDialogElementDto>
{
    public CreateDialogDialogElementDtoValidator(
        IValidator<LocalizationDto> localizationValidator,
        IValidator<CreateDialogDialogElementUrlDto> urlValidator)
    {
        // TODO: Valider at ID er på uuid v7 format dersom det er satt
        RuleFor(x => x.Id)
            .NotEqual(default(Guid))
            .NotEqual(x => x.RelatedDialogElementId);

        RuleFor(x => x.Type)
            .IsValidUri()
            .MaximumLength(Constants.DefaultMaxUriLength);

        RuleFor(x => x.AuthorizationAttribute)
            .MaximumLength(Constants.DefaultMaxStringLength);
        RuleForEach(x => x.DisplayName).SetValidator(localizationValidator);
        RuleFor(x => x.Urls)
            .NotEmpty()
            .ForEach(x => x.SetValidator(urlValidator));
    }
}

internal sealed class CreateDialogDialogElementUrlDtoValidator : AbstractValidator<CreateDialogDialogElementUrlDto>
{
    public CreateDialogDialogElementUrlDtoValidator()
    {
        RuleFor(x => x.Url)
            .NotNull()
            .IsValidUri()
            .MaximumLength(Constants.DefaultMaxUriLength);
        RuleFor(x => x.MimeType).MaximumLength(Constants.DefaultMaxStringLength);
        RuleFor(x => x.ConsumerTypeId).IsInEnum();
    }
}

internal sealed class CreateDialogDialogGuiActionDtoValidator : AbstractValidator<CreateDialogDialogGuiActionDto>
{
    public CreateDialogDialogGuiActionDtoValidator(
        IValidator<LocalizationDto> localizationValidator)
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
        RuleFor(x => x.PriorityId).IsInEnum();
        RuleFor(x => x.Title)
            .NotEmpty()
            .ForEach(x => x.SetValidator(localizationValidator));
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
        // TODO: Should we validate valid HttpMethods?
        RuleFor(x => x.HttpMethod)
            .NotEmpty()
            .MaximumLength(Constants.DefaultMaxStringLength);
        RuleFor(x => x.DocumentationUrl).IsValidUri().MaximumLength(Constants.DefaultMaxUriLength);
        RuleFor(x => x.RequestSchema).IsValidUri().MaximumLength(Constants.DefaultMaxUriLength);
        RuleFor(x => x.ResponseSchema).IsValidUri().MaximumLength(Constants.DefaultMaxUriLength);
        RuleFor(x => x.Deprecated)
            .Equal(true)
            .When(x => x.SunsetAt.HasValue);
    }
}

internal sealed class CreateDialogDialogActivityDtoValidator : AbstractValidator<CreateDialogDialogActivityDto>
{
    public CreateDialogDialogActivityDtoValidator(
        IValidator<LocalizationDto> localizationValidator)
    {
        // TODO: Valider at ID er på uuid v7 format dersom det er satt
        RuleFor(x => x.Id)
            .NotEqual(default(Guid))
            .NotEqual(x => x.RelatedActivityId);
        RuleFor(x => x.CreatedAt).LessThanOrEqualTo(DateTimeOffset.UtcNow);
        RuleFor(x => x.ExtendedType)
            .IsValidUri()
            .MaximumLength(Constants.DefaultMaxUriLength);
        RuleFor(x => x.TypeId).IsInEnum();
        RuleForEach(x => x.PerformedBy)
            .SetValidator(localizationValidator);
        RuleFor(x => x.Description)
            .NotEmpty()
            .ForEach(x => x.SetValidator(localizationValidator));
    }
}
