using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using System.Globalization;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;

internal sealed class LocalizationService : ILocalizationService
{
    private readonly IMapper _mapper;

    public LocalizationService(IMapper mapper)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<TLocalizationSet?> Merge<TLocalizationSet>(TLocalizationSet? set, List<LocalizationDto> dtos, CancellationToken cancellationToken = default)
        where TLocalizationSet : LocalizationSet, new()
    {
        set ??= new TLocalizationSet();
        await set.Localizations.MergeAsync(
            sources: dtos,
            destinationKeySelector: x => x.CultureCode.ToLower(CultureInfo.InvariantCulture),
            sourceKeySelector: x => x.CultureCode.ToLower(CultureInfo.InvariantCulture),
            create: CreateLocalization,
            update: UpdateLocalization,
            delete: DeleteLocalization,
            cancellationToken: cancellationToken);

        if (set.Localizations.Count == 0)
        {
            return null;
        }

        return set;
    }

    private Task<IEnumerable<Localization>> CreateLocalization(IEnumerable<LocalizationDto> creatables, CancellationToken cancellationToken)
    {
        var result = _mapper.Map<List<Localization>>(creatables);
        return Task.FromResult<IEnumerable<Localization>>(result);
    }

    private Task UpdateLocalization(IEnumerable<IUpdateSet<Localization, LocalizationDto>> updateSets, CancellationToken cancellationToken)
    {
        foreach (var (source, destination) in updateSets)
        {
            _mapper.Map(source, destination);
        }

        return Task.CompletedTask;
    }

    private Task DeleteLocalization(IEnumerable<Localization> deletables, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
