using AutoMapper;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Content;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Elements;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Create;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateDialogDto, DialogEntity>()
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.Status));

        CreateMap<CreateDialogDialogElementDto, DialogElement>();

        CreateMap<CreateDialogSearchTagDto, DialogSearchTag>();

        CreateMap<CreateDialogDialogElementUrlDto, DialogElementUrl>()
            .ForMember(dest => dest.ConsumerType, opt => opt.Ignore())
            .ForMember(dest => dest.ConsumerTypeId, opt => opt.MapFrom(src => src.ConsumerType));

        CreateMap<CreateDialogDialogGuiActionDto, DialogGuiAction>()
            .ForMember(dest => dest.Priority, opt => opt.Ignore())
            .ForMember(dest => dest.PriorityId, opt => opt.MapFrom(src => src.Priority))
            .ForMember(dest => dest.HttpMethod, opt => opt.Ignore())
            .ForMember(dest => dest.HttpMethodId, opt => opt.MapFrom(src => src.HttpMethod));

        CreateMap<CreateDialogDialogApiActionDto, DialogApiAction>();

        CreateMap<CreateDialogDialogApiActionEndpointDto, DialogApiActionEndpoint>()
            .ForMember(dest => dest.HttpMethod, opt => opt.Ignore())
            .ForMember(dest => dest.HttpMethodId, opt => opt.MapFrom(src => src.HttpMethod));

        CreateMap<CreateDialogDialogActivityDto, DialogActivity>()
            .ForMember(dest => dest.Type, opt => opt.Ignore())
            .ForMember(dest => dest.TypeId, opt => opt.MapFrom(src => src.Type));

        CreateMap<CreateDialogContentDto, DialogContent>()
            .ForMember(dest => dest.Type, opt => opt.Ignore())
            .ForMember(dest => dest.TypeId, opt => opt.MapFrom(src => src.Type));
    }
}
