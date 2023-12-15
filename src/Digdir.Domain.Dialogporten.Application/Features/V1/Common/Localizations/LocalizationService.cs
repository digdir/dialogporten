using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.Extensions.Enumerables;
using Digdir.Domain.Dialogporten.Domain.Localizations;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;

internal sealed class LocalizationService : ILocalizationService
{
    private readonly IMapper _mapper;

    public LocalizationService(IMapper mapper)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public TLocalizationSet? Merge<TLocalizationSet>(TLocalizationSet? set, List<LocalizationDto> dtos)
        where TLocalizationSet : LocalizationSet, new()
    {
        set ??= new TLocalizationSet();
        set.Localizations.Merge(
            sources: dtos,
            destinationKeySelector: x => x.CultureCode,
            sourceKeySelector: x => x.CultureCode,
            create: _mapper.Map<List<Localization>>,
            update: UpdateLocalization,
            delete: DeleteDelegate.NoOp,
            comparer: StringComparer.InvariantCultureIgnoreCase);

        return set.Localizations.Count == 0
            ? null
            : set;
    }

    private void UpdateLocalization(IEnumerable<UpdateSet<Localization, LocalizationDto>> updateSets)
    {
        foreach (var (source, destination) in updateSets)
        {
            _mapper.Map(source, destination);
        }
    }
}
