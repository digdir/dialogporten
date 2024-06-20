using Digdir.Domain.Dialogporten.Domain.Localizations;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;

public sealed class LocalizationDto
{
    private readonly string _languageCode = null!;

    public required string Value { get; init; }
    public required string LanguageCode
    {
        get => _languageCode;
        init => _languageCode = Localization.NormalizeCultureCode(value)!;
    }
}
