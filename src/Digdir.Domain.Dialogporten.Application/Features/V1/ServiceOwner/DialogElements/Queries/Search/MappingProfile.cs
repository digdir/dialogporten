using AutoMapper;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Elements;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogElements.Queries.Search;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DialogElement, SearchDialogElementDto>()
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.Dialog.DeletedAt));

    }
}