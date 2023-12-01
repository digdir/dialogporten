using AutoMapper;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogElements.Queries.Get;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DialogElement, GetDialogElementDto>();

        CreateMap<DialogElementUrl, GetDialogElementUrlDto>()
            .ForMember(dest => dest.ConsumerType, opt => opt
                .MapFrom(src => src.ConsumerTypeId));
    }
}
