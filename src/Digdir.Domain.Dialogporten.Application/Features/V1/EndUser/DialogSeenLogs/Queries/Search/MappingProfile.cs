using AutoMapper;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogSeenLogs.Queries.Search;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // todo: additional mapping or change properties
        CreateMap<DialogActor, SearchDialogSeenLogDto>()
            .ForMember(dest => dest.SeenAt, opt => opt.MapFrom(src => src.CreatedAt));
    }
}
