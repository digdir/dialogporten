using Digdir.Domain.Dialogporten.Application.Common.Extensions.FluentValidation;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;

internal sealed class LocalizationDtosValidator : AbstractValidator<IEnumerable<LocalizationDto>>
{
    public LocalizationDtosValidator(int maximumLength = 255)
    {
        RuleFor(x => x)
            .UniqueBy(x => x.CultureCode)
            .ForEach(x => x.SetValidator(new LocalizationDtoValidator(maximumLength)));
    }
}

internal sealed class LocalizationDtoValidator : AbstractValidator<LocalizationDto>
{
    public LocalizationDtoValidator(int maximumLength = 255)
    {
        RuleFor(x => x).NotNull();

        RuleFor(x => x.Value)
            .NotEmpty()
            .NotNull()
            .MaximumLength(maximumLength);

        RuleFor(x => x.CultureCode)
            .NotEmpty()
            .Must(x => x is null || Localization.IsValidCultureCode(x))
            .WithMessage("'{PropertyName}' must be a valid culture code.");
    }
}
