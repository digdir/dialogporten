using Digdir.Domain.Dialogporten.Application.Common.Extensions.FluentValidation;
using FluentValidation;
using System.Globalization;

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
    private static readonly HashSet<string> ValidCultureNames = CultureInfo
        .GetCultures(CultureTypes.NeutralCultures | CultureTypes.SpecificCultures)
        .Where(x => !string.IsNullOrWhiteSpace(x.Name))
        .Select(x => x.Name)
        .ToHashSet();

    public LocalizationDtoValidator(int maximumLength = 255)
    {
        RuleFor(x => x).NotNull();

        RuleFor(x => x.Value)
            .NotNull()
            .MaximumLength(maximumLength);

        RuleFor(x => x.CultureCode)
            .NotEmpty()
            .Must(x => ValidCultureNames.Contains(x.Trim().Replace('_', '-'), StringComparer.InvariantCultureIgnoreCase))
            .WithMessage("'{PropertyName}' must be a valid culture code.");
    }
}
