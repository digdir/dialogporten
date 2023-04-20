using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using System.Globalization;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;

internal sealed class LocalizationService : ILocalizationService
{
    private readonly IMapper _mapper;
    private readonly IDialogueDbContext _db;

    public LocalizationService(IMapper mapper, IDialogueDbContext db)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task Merge(LocalizationSet set, List<LocalizationDto> dtos, CancellationToken cancellationToken = default)
    {
        set.Localizations = await set.Localizations.MergeAsync(
            sources: dtos,
            destinationKeySelector: x => x.CultureCode.ToLower(CultureInfo.InvariantCulture),
            sourceKeySelector: x => x.CultureCode.ToLower(CultureInfo.InvariantCulture),
            create: CreateLocalization,
            update: UpdateLocalization,
            delete: DeleteLocalization,
            cancellationToken: cancellationToken);
    }

    private Task<IEnumerable<Localization>> CreateLocalization(IEnumerable<LocalizationDto> creatables, CancellationToken cancellationToken)
    {
        var result = _mapper.Map<List<Localization>>(creatables);
        return Task.FromResult<IEnumerable<Localization>>(result);
    }

    private Task UpdateLocalization(IEnumerable<IUpdateSet<Localization, LocalizationDto>> updateSets, CancellationToken cancellationToken)
    {
        foreach (var updateSet in updateSets)
        {
            _mapper.Map(updateSet.Source, updateSet.Destination);
        }

        return Task.CompletedTask;
    }

    private Task DeleteLocalization(IEnumerable<Localization> deletables, CancellationToken cancellationToken)
    {
        _db.Localizations.RemoveRange(deletables);
        return Task.CompletedTask;
    }
}
