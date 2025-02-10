using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Domain;
using Digdir.Domain.Dialogporten.Domain.Actors;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Common.Actors;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {

        var actorDtoType = typeof(ActorDto);
        var actorType = typeof(Actor);

        var derivedActorTypes = DomainAssemblyMarker
            .Assembly
            .GetTypes()
            .Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(actorType))
            .ToList();

        CreateMap<Actor, ActorDto>()
            .ForMember(dest => dest.ActorType, opt => opt.MapFrom(src => src.ActorTypeId))
            .ForMember(dest => dest.ActorId, opt => opt.MapFrom(src => IdentifierMasker.GetMaybeMaskedIdentifier(src.ActorId)))
            .ForMember(dest => dest.ActorName, opt => opt.MapFrom(src => src.ActorNameEntity == null ? src.ActorName! : src.ActorNameEntity.Name));

        foreach (var outputActor in derivedActorTypes)
        {
            CreateMap(outputActor, actorDtoType)
                .IncludeBase(actorType, actorDtoType);
        }
    }
}
