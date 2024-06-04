using AutoMapper;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Content;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Elements;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Get;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DialogEntity, GetDialogDto>()
            .ForMember(dest => dest.Revision, opt => opt.MapFrom(src => src.Revision))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.StatusId))
            .ForMember(dest => dest.SeenSinceLastUpdate, opt => opt.Ignore());

        CreateMap<DialogSeenLog, GetDialogDialogSeenLogDto>()
            .ForMember(dest => dest.SeenAt, opt => opt.MapFrom(src => src.CreatedAt));

        CreateMap<DialogActivity, GetDialogDialogActivityDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.TypeId));

        CreateMap<DialogApiAction, GetDialogDialogApiActionDto>();

        CreateMap<DialogApiActionEndpoint, GetDialogDialogApiActionEndpointDto>()
            .ForMember(dest => dest.HttpMethod, opt => opt.MapFrom(src => src.HttpMethodId));

        CreateMap<DialogGuiAction, GetDialogDialogGuiActionDto>()
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.PriorityId));

        CreateMap<DialogElement, GetDialogDialogElementDto>();

        CreateMap<DialogElementUrl, GetDialogDialogElementUrlDto>()
            .ForMember(dest => dest.ConsumerType, opt => opt.MapFrom(src => src.ConsumerTypeId));

        CreateMap<DialogContent, GetDialogContentDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.TypeId));
    }

}
