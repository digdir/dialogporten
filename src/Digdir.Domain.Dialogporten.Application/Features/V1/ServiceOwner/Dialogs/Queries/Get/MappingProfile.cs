using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Content;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actors;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Attachments;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Contents;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Get;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DialogEntity, GetDialogDto>()
            .ForMember(dest => dest.Revision, opt => opt.MapFrom(src => src.Revision))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.StatusId))
            .ForMember(dest => dest.SeenSinceLastUpdate, opt => opt.Ignore());

        CreateMap<DialogSeenLogSeenByActor, GetDialogDialogSeenLogSeenByActorDto>();
        CreateMap<DialogSeenLog, GetDialogDialogSeenLogDto>()
            .ForMember(dest => dest.SeenAt, opt => opt.MapFrom(src => src.CreatedAt));

        CreateMap<DialogActivity, GetDialogDialogActivityDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.TypeId));
        CreateMap<DialogActivityPerformedByActor, GetDialogDialogActivityPerformedByActorDto>()
            .ForMember(dest => dest.ActorType, opt => opt.MapFrom(src => src.ActorTypeId));

        CreateMap<DialogApiAction, GetDialogDialogApiActionDto>();
        CreateMap<DialogApiActionEndpoint, GetDialogDialogApiActionEndpointDto>()
            .ForMember(dest => dest.HttpMethod, opt => opt.MapFrom(src => src.HttpMethodId));

        CreateMap<DialogGuiAction, GetDialogDialogGuiActionDto>()
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.PriorityId))
            .ForMember(dest => dest.HttpMethod, opt => opt.MapFrom(src => src.HttpMethodId));

        CreateMap<DialogAttachment, GetDialogDialogAttachmentDto>();
        CreateMap<AttachmentUrl, GetDialogDialogAttachmentUrlDto>()
            .ForMember(dest => dest.ConsumerType, opt => opt.MapFrom(src => src.ConsumerTypeId));

        CreateMap<DialogSearchTag, GetDialogSearchTagDto>();

        CreateMap<List<DialogContent>?, GetDialogContentDto?>()
            .ConvertUsing<DialogContentOutputConverter<GetDialogContentDto>>();

        CreateMap<DialogTransmissionSenderActor, GetDialogDialogTransmissionSenderActorDto>()
            .ForMember(dest => dest.ActorType, opt => opt.MapFrom(src => src.ActorTypeId));

        CreateMap<List<TransmissionContent>?, GetDialogDialogTransmissionContentDto?>()
            .ConvertUsing<TransmissionContentOutputConverter<GetDialogDialogTransmissionContentDto>>();

        CreateMap<DialogTransmission, GetDialogDialogTransmissionDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.TypeId));

        CreateMap<TransmissionAttachment, GetDialogTransmissionAttachmentDto>();
        CreateMap<AttachmentUrl, GetDialogTransmissionAttachmentUrlDto>()
            .ForMember(dest => dest.ConsumerType, opt => opt.MapFrom(src => src.ConsumerTypeId));
    }
}
