using AutoMapper;
using Digdir.Domain.Dialogporten.Domain.Localizations;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Out
        CreateMap<LocalizationSet?, List<LocalizationDto>?>()
            .ConvertUsing(src => src == null ? null :
                src.Localizations
                    .Select(x => new LocalizationDto { CultureCode = x.CultureCode, Value = x.Value })
                    .ToList());

        // In
        CreateMap<LocalizationDto, Localization>();

        // Create incoming mappings for all types derived from LocalizationSet
        var derivedLocalizationSetTypes = typeof(LocalizationSet).Assembly.GetTypes()
            .Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(LocalizationSet)))
            .ToList();
        var incomingSetMap = CreateMap<IEnumerable<LocalizationDto>, LocalizationSet>()
           .ForMember(dest => dest.Localizations, opt => opt.MapFrom(src => src));
        foreach (var derivedSetType in derivedLocalizationSetTypes)
        {
            CreateMap(typeof(IEnumerable<LocalizationDto>), derivedSetType);
            incomingSetMap = incomingSetMap.Include(typeof(IEnumerable<LocalizationDto>), derivedSetType);
        }
    }
}
