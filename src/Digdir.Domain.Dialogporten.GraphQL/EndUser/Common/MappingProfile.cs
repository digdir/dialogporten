using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Search;

namespace Digdir.Domain.Dialogporten.GraphQL.EndUser.Common;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<LocalizationDto, Localization>();

        CreateMap<GetDialogDialogSeenLogDto, SeenLog>();
        CreateMap<SearchDialogDialogSeenLogDto, SeenLog>();

        CreateMap<GetDialogContentDto, Content>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type));
        CreateMap<SearchDialogContentDto, Content>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type));

        CreateMap<GetDialogDialogActivityDto, Activity>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type));
        CreateMap<SearchDialogDialogActivityDto, Activity>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type));
    }
}
