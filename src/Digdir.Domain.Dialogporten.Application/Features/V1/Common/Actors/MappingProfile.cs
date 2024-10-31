using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Actors;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        // CreateMap<ActorDto, DialogSeenLogSeenByActor>()
        //     .ForMember(dest => dest.ActorTypeId, opt => opt.MapFrom(src => src.ActorType));

        // CreateMap<DialogSeenLogSeenByActor, ActorDto>()
        //     .ForMember(dest => dest.ActorType, opt => opt.MapFrom(src => src.ActorTypeId))
        //     .ForMember(dest => dest.ActorId, opt => opt.MapFrom(src => IdentifierMasker.GetMaybeMaskedIdentifier(src.ActorId)));
        //
        // CreateMap<DialogActivityPerformedByActor, ActorDto>()
        //     .ForMember(dest => dest.ActorType, opt => opt.MapFrom(src => src.ActorTypeId));
        // .ForMember(dest => dest.ActorId, opt => opt.MapFrom(src => IdentifierMasker.GetMaybeMaskedIdentifier(src.ActorId)));
        // CreateMap<DialogSeenLogSeenByActor, ActorDto>()
        //     .ForMember(dest => dest.ActorType, opt => opt.MapFrom(src => src.ActorTypeId))
        //     .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

        // CreateMap<DialogActivityPerformedByActor, ActorDto>()
        //     .ForMember(dest => dest.ActorType, opt => opt.MapFrom(src => src.ActorTypeId));
        //
        // CreateMap<DialogSeenLogSeenByActor, ActorDto>()
        //     .ForMember(dest => dest.ActorType, opt => opt.MapFrom(src => src.ActorTypeId));
        //
        // CreateMap<DialogTransmissionSenderActor, ActorDto>()
        //     .ForMember(dest => dest.ActorType, opt => opt.MapFrom(src => src.ActorTypeId));
    }
}
