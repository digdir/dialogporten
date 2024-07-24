using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Content;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Search;

namespace Digdir.Domain.Dialogporten.GraphQL.EndUser.Common;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<LocalizationDto, Localization>();

        CreateMap<DialogContentValueDto, ContentValue>();

        CreateMap<GetDialogDialogSeenLogDto, SeenLog>();
        CreateMap<GetDialogDialogSeenLogActorDto, Actor>();

        CreateMap<SearchDialogDialogSeenLogDto, SeenLog>();
        CreateMap<SearchDialogDialogSeenLogActorDto, Actor>();

        CreateMap<GetDialogDialogActivityDto, Activity>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type));
        CreateMap<GetDialogDialogActivityActorDto, Actor>()
            .ForMember(dest => dest.ActorType, opt => opt.MapFrom(src => src.ActorType));

        CreateMap<SearchDialogDialogActivityDto, Activity>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type));
        CreateMap<SearchDialogDialogActivityActorDto, Actor>()
            .ForMember(dest => dest.ActorType, opt => opt.MapFrom(src => src.ActorType));
    }
}
