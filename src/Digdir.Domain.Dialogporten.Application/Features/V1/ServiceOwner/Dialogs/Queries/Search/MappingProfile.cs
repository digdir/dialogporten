using AutoMapper;
using Digdir.Domain.Dialogporten.Domain.Actors;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Content;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Search;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DialogEntity, SearchDialogDto>()
            .ForMember(dest => dest.SeenSinceLastUpdate, opt => opt.MapFrom(src => src.SeenLog
                .Where(x => x.CreatedAt >= x.Dialog.UpdatedAt)
                .OrderByDescending(x => x.CreatedAt)
            ))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content.Where(x => x.Type.OutputInList)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.StatusId));

        CreateMap<Actor, SearchDialogDialogActorDto>()
            .ForMember(dest => dest.ActorId, opt => opt.MapFrom(src => src.ActorId));

        CreateMap<DialogSearchTag, SearchDialogSearchTagDto>();

        CreateMap<DialogContent, SearchDialogContentDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.TypeId));
    }
}
