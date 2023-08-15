using Digdir.Domain.Dialogporten.Domain.Localizations;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;

public sealed class LocalizationDto
{
    private string _cultureCode = null!;

    public required string Value { get; init; }
    public required string CultureCode 
    { 
        get => _cultureCode; 
        init => _cultureCode = Localization.NormalizeCultureCode(value)!; 
    }
}