using Digdir.Domain.Dialogporten.Domain.Localizations;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;

internal interface ILocalizationService
{
    Task<TLocalizationSet?> Merge<TLocalizationSet>(TLocalizationSet? set, List<LocalizationDto> dtos, CancellationToken cancellationToken = default)
        where TLocalizationSet : LocalizationSet, new();
}