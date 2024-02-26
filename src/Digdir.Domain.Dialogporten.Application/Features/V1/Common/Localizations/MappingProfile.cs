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
        var localizationDtoEnumerableType = typeof(IEnumerable<LocalizationDto>);
        var localizationSetType = typeof(LocalizationSet);
        var openLocalizationSetConverter = typeof(LocalizationSetConverter<>);
        var derivedLocalizationSetTypes = localizationSetType
            .Assembly
            .GetTypes()
            .Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(localizationSetType))
            .ToList();
        foreach (var derivedSetType in derivedLocalizationSetTypes)
        {
            CreateMap(localizationDtoEnumerableType, derivedSetType)
                .ConvertUsing(openLocalizationSetConverter.MakeGenericType(derivedSetType));
        }
    }
}
