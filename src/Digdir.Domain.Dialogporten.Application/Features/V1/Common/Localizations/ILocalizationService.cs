using Digdir.Domain.Dialogporten.Domain.Localizations;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;

internal interface ILocalizationService
{
    Task Merge(LocalizationSet entities, List<LocalizationDto> dtos, CancellationToken cancellationToken = default);
}