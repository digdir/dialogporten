using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Content;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using DialogActivityDto = Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Search.DialogActivityDto;
using DialogActivityPerformedByActorDto = Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Search.DialogActivityPerformedByActorDto;
using DialogSeenLogDto = Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Search.DialogSeenLogDto;
using DialogSeenLogSeenByActorDto = Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Search.DialogSeenLogSeenByActorDto;

namespace Digdir.Domain.Dialogporten.GraphQL.EndUser.Common;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<LocalizationDto, Localization>();

        CreateMap<ContentValueDto, ContentValue>();

        CreateMap<Application.Features.V1.EndUser.Dialogs.Queries.Get.DialogSeenLogDto, SeenLog>();
        CreateMap<Application.Features.V1.EndUser.Dialogs.Queries.Get.DialogSeenLogSeenByActorDto, Actor>();

        CreateMap<DialogSeenLogDto, SeenLog>();
        CreateMap<DialogSeenLogSeenByActorDto, Actor>();

        CreateMap<Application.Features.V1.EndUser.Dialogs.Queries.Get.DialogActivityDto, Activity>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type));
        CreateMap<Application.Features.V1.EndUser.Dialogs.Queries.Get.DialogActivityPerformedByActorDto, Actor>()
            .ForMember(dest => dest.ActorType, opt => opt.MapFrom(src => src.ActorType));

        CreateMap<DialogActivityDto, Activity>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type));
        CreateMap<DialogActivityPerformedByActorDto, Actor>()
            .ForMember(dest => dest.ActorType, opt => opt.MapFrom(src => src.ActorType));
    }
}
