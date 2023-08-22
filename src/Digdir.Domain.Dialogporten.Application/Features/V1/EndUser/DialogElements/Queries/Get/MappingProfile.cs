using AutoMapper;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogElements.Queries.Get;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DialogElement, GetDialogElementDto>();
        CreateMap<DialogElement, GetRelatedDialogElementDto>();
        CreateMap<DialogApiAction, GetDialogApiActionDto>();
        
        CreateMap<DialogApiActionEndpoint, GetDialogApiActionEndpointDto>()
            .ForMember(dest => dest.HttpMethod, opt => opt
                .MapFrom(src => src.HttpMethodId));
        
        CreateMap<DialogElementUrl, GetDialogElementUrlDto>()
            .ForMember(dest => dest.ConsumerType, opt => opt
                .MapFrom(src => src.ConsumerTypeId));
        
        CreateMap<DialogActivity, GetDialogActivityDto>()
            .ForMember(dest => dest.Type, opt => opt
                .MapFrom(src => src.TypeId));
    }
}