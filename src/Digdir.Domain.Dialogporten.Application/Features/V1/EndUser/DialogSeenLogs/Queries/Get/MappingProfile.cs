using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogSeenLogs.Queries.Get;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DialogSeenLog, DialogSeenLogDto>()
            .ForMember(dest => dest.SeenAt, opt => opt.MapFrom(src => src.CreatedAt));

        CreateMap<DialogSeenLogSeenByActor, SeenLogSeenByActorDto>()
            .ForMember(dest => dest.ActorId,
                opt => opt.MapFrom(src => IdentifierMasker.GetMaybeMaskedIdentifier(src.ActorId)));
    }
}
