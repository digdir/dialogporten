using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;

internal sealed class LocalizationDtoValidator : AbstractValidator<LocalizationDto>
{
    public LocalizationDtoValidator(int maximumLength = 255)
    {
        RuleFor(x => x).NotNull();

        RuleFor(x => x.Value)
            .NotNull()
            .MaximumLength(maximumLength);

        // TODO: Validate correct culture code?
        RuleFor(x => x.CultureCode)
            .NotEmpty();
    }
}
