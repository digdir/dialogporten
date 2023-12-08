using AutoMapper;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Elements;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogElements.Queries.Get;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DialogElement, GetDialogElementDto>()
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.Dialog.DeletedAt));

        CreateMap<DialogElementUrl, GetDialogElementUrlDto>()
            .ForMember(dest => dest.ConsumerType, opt => opt.MapFrom(src => src.ConsumerTypeId));
    }
}