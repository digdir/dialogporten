using AutoMapper;
using Digdir.Domain.Dialogporten.Domain.Localizations;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Out
        CreateMap<LocalizationSet, List<LocalizationDto>>()
            .ConvertUsing(src => src.Localizations
                .Select(x => new LocalizationDto { CultureCode = x.CultureCode, Value = x.Value })
                .ToList());

        // In
        CreateMap<IEnumerable<LocalizationDto>, LocalizationSet>()
            .ForMember(dest => dest.Localizations, opt => opt.MapFrom(src => src));
        CreateMap<LocalizationDto, Localization>();
    }
}
