using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Commands.Update;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        // TODO: should we ignore id for entities where user cannot set id on create?

        // In
        CreateMap<UpdateDialogDto, DialogEntity>()
            .IgnoreComplexDestinationProperties()
            .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.Status));

        CreateMap<UpdateDialogDialogApiActionDto, DialogApiAction>()
            .IgnoreComplexDestinationProperties()
            .ForMember(x => x.Id, opt => opt.Ignore());
        CreateMap<UpdateDialogDialogApiActionEndpointDto, DialogApiActionEndpoint>()
            .IgnoreComplexDestinationProperties()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(dest => dest.HttpMethodId, opt => opt.MapFrom(src => src.HttpMethod));

        CreateMap<UpdateDialogDialogGuiActionDto, DialogGuiAction>()
            .IgnoreComplexDestinationProperties()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PriorityId, opt => opt.MapFrom(src => src.Priority));

        CreateMap<UpdateDialogDialogElementDto, DialogElement>()
            .IgnoreComplexDestinationProperties();
        CreateMap<UpdateDialogDialogElementUrlDto, DialogElementUrl>()
            .IgnoreComplexDestinationProperties()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ConsumerTypeId, opt => opt.MapFrom(src => src.ConsumerType));

        // Since this is append only, we don't need to merge with existing
        // activity records and thus can map complex properties
        CreateMap<UpdateDialogDialogActivityDto, DialogActivity>()
            .ForMember(dest => dest.TypeId, opt => opt.MapFrom(src => src.Type));

        // To support json patch
        CreateMap<DialogEntity, UpdateDialogDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.StatusId));
        CreateMap<DialogActivity, UpdateDialogDialogActivityDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.TypeId));
        CreateMap<DialogApiAction, UpdateDialogDialogApiActionDto>();
        CreateMap<DialogApiActionEndpoint, UpdateDialogDialogApiActionEndpointDto>()
            .ForMember(dest => dest.HttpMethod, opt => opt.MapFrom(src => src.HttpMethodId));
        CreateMap<DialogGuiAction, UpdateDialogDialogGuiActionDto>()
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.PriorityId));
        CreateMap<DialogElement, UpdateDialogDialogElementDto>();
        CreateMap<DialogElementUrl, UpdateDialogDialogElementUrlDto>()
            .ForMember(dest => dest.ConsumerType, opt => opt.MapFrom(src => src.ConsumerTypeId));
    }
}
