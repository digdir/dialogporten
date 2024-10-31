using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Actors;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogSeenLogs.Queries.Search;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DialogSeenLog, SeenLogDto>()
            .ForMember(dest => dest.SeenAt, opt => opt.MapFrom(src => src.CreatedAt));

        CreateMap<DialogSeenLogSeenByActor, ActorDto>()
            .ForMember(dest => dest.ActorId, opt => opt.MapFrom(src => IdentifierMasker.GetMaybeMaskedIdentifier(src.ActorId)));
    }
}
