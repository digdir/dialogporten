using AutoMapper;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actors;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogSeenLogs.Queries.Search;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DialogSeenLog, SearchDialogSeenLogDto>()
            .ForMember(dest => dest.SeenAt, opt => opt.MapFrom(src => src.CreatedAt));

        CreateMap<DialogSeenLogSeenByActor, SearchDialogDialogSeenLogSeenByActorDto>();
    }
}
