using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Content;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.Domain.Attachments;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Contents;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions.Contents;

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
            .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content));

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
            .IgnoreComplexDestinationProperties()
            .ForMember(x => x.Id, opt => opt.Ignore());

        CreateMap<UpdateDialogDialogAttachmentUrlDto, AttachmentUrl>()
            .IgnoreComplexDestinationProperties()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ConsumerType, opt => opt.Ignore())
            .ForMember(dest => dest.ConsumerTypeId, opt => opt.MapFrom(src => src.ConsumerType));

        CreateMap<UpdateDialogContentDto?, List<DialogContent>?>()
            .ConvertUsing<DialogContentInputConverter<UpdateDialogContentDto>>();

        // Since these are append-only, we don't need to merge with existing
        // activity/transmission records and thus can map complex properties
        CreateMap<UpdateDialogDialogActivityDto, DialogActivity>()
            .ForMember(dest => dest.Type, opt => opt.Ignore())
            .ForMember(dest => dest.TypeId, opt => opt.MapFrom(src => src.Type));

        CreateMap<UpdateDialogDialogActivityPerformedByActorDto, DialogActivityPerformedByActor>()
            .ForMember(dest => dest.ActorType, opt => opt.Ignore())
            .ForMember(dest => dest.ActorTypeId, opt => opt.MapFrom(src => src.ActorType));

        CreateMap<UpdateDialogDialogTransmissionDto, DialogTransmission>()
            .ForMember(dest => dest.Type, opt => opt.Ignore())
            .ForMember(dest => dest.TypeId, opt => opt.MapFrom(src => src.Type));

        CreateMap<UpdateDialogDialogTransmissionContentDto?, List<DialogTransmissionContent>?>()
            .ConvertUsing<TransmissionContentInputConverter<UpdateDialogDialogTransmissionContentDto>>();

        CreateMap<UpdateDialogDialogTransmissionSenderActorDto, DialogTransmissionSenderActor>()
            .ForMember(dest => dest.ActorType, opt => opt.Ignore())
            .ForMember(dest => dest.ActorTypeId, opt => opt.MapFrom(src => src.ActorType));

        CreateMap<UpdateDialogTransmissionAttachmentDto, DialogTransmissionAttachment>();

        CreateMap<UpdateDialogTransmissionAttachmentUrlDto, AttachmentUrl>()
            .ForMember(dest => dest.ConsumerType, opt => opt.Ignore())
            .ForMember(dest => dest.ConsumerTypeId, opt => opt.MapFrom(src => src.ConsumerType));

        // ===========================================
        // ================== Patch ==================
        // ===========================================
        CreateMap<GetDialogDto, UpdateDialogDto>()
            // Remove all existing activities and transmissions, since these lists are append only and
            // existing activities/transmissions should not be considered in the update request.
            .ForMember(dest => dest.Activities, opt => opt.Ignore())
            .ForMember(dest => dest.Transmissions, opt => opt.Ignore());
        CreateMap<GetDialogSearchTagDto, UpdateDialogSearchTagDto>();
        CreateMap<GetDialogDialogActivityDto, UpdateDialogDialogActivityDto>();
        CreateMap<GetDialogDialogActivityPerformedByActorDto, UpdateDialogDialogActivityPerformedByActorDto>();
        CreateMap<GetDialogDialogApiActionDto, UpdateDialogDialogApiActionDto>();
        CreateMap<GetDialogDialogApiActionEndpointDto, UpdateDialogDialogApiActionEndpointDto>();
        CreateMap<GetDialogDialogGuiActionDto, UpdateDialogDialogGuiActionDto>();
        CreateMap<GetDialogDialogAttachmentDto, UpdateDialogDialogAttachmentDto>();
        CreateMap<GetDialogDialogAttachmentUrlDto, UpdateDialogDialogAttachmentUrlDto>();
        CreateMap<GetDialogContentDto, UpdateDialogContentDto>();
    }
}
