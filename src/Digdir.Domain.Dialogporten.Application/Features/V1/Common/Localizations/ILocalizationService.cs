using Digdir.Domain.Dialogporten.Domain.Localizations;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;

internal interface ILocalizationService
{
    TLocalizationSet? Merge<TLocalizationSet>(TLocalizationSet? set, List<LocalizationDto> dtos)
        where TLocalizationSet : LocalizationSet, new();
}