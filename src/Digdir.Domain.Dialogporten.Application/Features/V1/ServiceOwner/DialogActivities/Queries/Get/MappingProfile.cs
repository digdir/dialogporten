using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Actors;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogActivities.Queries.Get;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DialogActivity, ActivityDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.TypeId))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.Dialog.DeletedAt));

        CreateMap<DialogActivityPerformedByActor, ActorDto>()
            .ForMember(dest => dest.ActorId, opt => opt.MapFrom(src => IdentifierMasker.GetMaybeMaskedIdentifier(src.ActorId)))
            .ForMember(dest => dest.ActorType, opt => opt.MapFrom(src => src.ActorTypeId));

    }
}
