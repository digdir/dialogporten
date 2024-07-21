using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actors;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Attachments;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Content;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Update;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ===========================================
        // ================== In =====================
        // ===========================================
        CreateMap<UpdateDialogDto, DialogEntity>()
            .IgnoreComplexDestinationProperties()
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.Status));

        CreateMap<UpdateDialogSearchTagDto, DialogSearchTag>()
            .IgnoreComplexDestinationProperties()
            .ForMember(x => x.Id, opt => opt.Ignore());

        CreateMap<UpdateDialogDialogApiActionDto, DialogApiAction>()
            .IgnoreComplexDestinationProperties()
            .ForMember(x => x.Id, opt => opt.Ignore());

        CreateMap<UpdateDialogDialogApiActionEndpointDto, DialogApiActionEndpoint>()
            .IgnoreComplexDestinationProperties()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(dest => dest.HttpMethod, opt => opt.Ignore())
            .ForMember(dest => dest.HttpMethodId, opt => opt.MapFrom(src => src.HttpMethod));

        CreateMap<UpdateDialogDialogGuiActionDto, DialogGuiAction>()
            .IgnoreComplexDestinationProperties()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Priority, opt => opt.Ignore())
            .ForMember(dest => dest.PriorityId, opt => opt.MapFrom(src => src.Priority))
            .ForMember(dest => dest.HttpMethod, opt => opt.Ignore())
            .ForMember(dest => dest.HttpMethodId, opt => opt.MapFrom(src => src.HttpMethod));

        CreateMap<UpdateDialogDialogAttachmentDto, DialogAttachment>()
            .IgnoreComplexDestinationProperties();

        CreateMap<UpdateDialogDialogAttachmentUrlDto, DialogAttachmentUrl>()
            .IgnoreComplexDestinationProperties()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ConsumerType, opt => opt.Ignore())
            .ForMember(dest => dest.ConsumerTypeId, opt => opt.MapFrom(src => src.ConsumerType));

        CreateMap<UpdateDialogContentDto, DialogContent>()
            .IgnoreComplexDestinationProperties()
            .ForMember(dest => dest.Type, opt => opt.Ignore())
            .ForMember(dest => dest.TypeId, opt => opt.MapFrom(src => src.Type));

        // Since this is append only, we don't need to merge with existing
        // activity records and thus can map complex properties
        CreateMap<UpdateDialogDialogActivityDto, DialogActivity>()
            .ForMember(dest => dest.Type, opt => opt.Ignore())
            .ForMember(dest => dest.TypeId, opt => opt.MapFrom(src => src.Type));

        CreateMap<UpdateDialogDialogActivityActorDto, DialogActor>()
            .ForMember(dest => dest.ActorType, opt => opt.Ignore())
            .ForMember(dest => dest.ActorTypeId, opt => opt.MapFrom(src => src.ActorType));

        // ===========================================
        // ================== Patch ==================
        // ===========================================
        CreateMap<GetDialogDto, UpdateDialogDto>()
            // Remove all existing activities, since this list is append only and
            // existing activities should not be considered in the update request.
            .ForMember(dest => dest.Activities, opt => opt.Ignore());
        CreateMap<GetDialogSearchTagDto, UpdateDialogSearchTagDto>();
        CreateMap<GetDialogDialogActivityDto, UpdateDialogDialogActivityDto>();
        CreateMap<GetDialogDialogActivityActorDto, UpdateDialogDialogActivityActorDto>();
        CreateMap<GetDialogDialogApiActionDto, UpdateDialogDialogApiActionDto>();
        CreateMap<GetDialogDialogApiActionEndpointDto, UpdateDialogDialogApiActionEndpointDto>();
        CreateMap<GetDialogDialogGuiActionDto, UpdateDialogDialogGuiActionDto>();
        CreateMap<GetDialogDialogAttachmentDto, UpdateDialogDialogAttachmentDto>();
        CreateMap<GetDialogDialogAttachmentUrlDto, UpdateDialogDialogAttachmentUrlDto>();
        CreateMap<GetDialogContentDto, UpdateDialogContentDto>();
    }
}
