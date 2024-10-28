using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogActivities.Queries.Get;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DialogActivity, DialogActivityDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.TypeId));

        CreateMap<DialogActivityPerformedByActor, PerformedByActorDto>()
            .ForMember(dest => dest.ActorType, opt => opt.MapFrom(src => src.ActorTypeId))
            .ForMember(dest => dest.ActorId, opt => opt.MapFrom(src => IdentifierMasker.GetMaybeMaskedIdentifier(src.ActorId)));
    }
}
