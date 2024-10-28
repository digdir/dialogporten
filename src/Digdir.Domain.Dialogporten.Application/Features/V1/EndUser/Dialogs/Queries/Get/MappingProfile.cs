using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Content;
using Digdir.Domain.Dialogporten.Domain.Attachments;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Contents;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions.Contents;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Get;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DialogEntity, DialogDto>()
            .ForMember(dest => dest.Revision, opt => opt.MapFrom(src => src.Revision))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.StatusId))
            .ForMember(dest => dest.SeenSinceLastUpdate, opt => opt.Ignore())
            .ForMember(dest => dest.SystemLabel, opt => opt.MapFrom(src => src.DialogEndUserContext.SystemLabelId));

        CreateMap<DialogSeenLog, DialogSeenLogDto>()
            .ForMember(dest => dest.SeenAt, opt => opt.MapFrom(src => src.CreatedAt));

        CreateMap<DialogSeenLogSeenByActor, DialogSeenLogSeenByActorDto>()
            .ForMember(dest => dest.ActorId, opt => opt.MapFrom(src => IdentifierMasker.GetMaybeMaskedIdentifier(src.ActorId)));

        CreateMap<DialogActivity, DialogActivityDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.TypeId));

        CreateMap<DialogActivityPerformedByActor, DialogActivityPerformedByActorDto>()
            .ForMember(dest => dest.ActorType, opt => opt.MapFrom(src => src.ActorTypeId))
            .ForMember(dest => dest.ActorId, opt => opt.MapFrom(src => IdentifierMasker.GetMaybeMaskedIdentifier(src.ActorId)));

        CreateMap<DialogApiAction, DialogApiActionDto>();

        CreateMap<DialogApiActionEndpoint, DialogApiActionEndpointDto>()
            .ForMember(dest => dest.HttpMethod, opt => opt.MapFrom(src => src.HttpMethodId));

        CreateMap<DialogGuiAction, DialogGuiActionDto>()
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.PriorityId))
            .ForMember(dest => dest.HttpMethod, opt => opt.MapFrom(src => src.HttpMethodId));

        CreateMap<DialogAttachment, DialogAttachmentDto>();

        CreateMap<AttachmentUrl, DialogAttachmentUrlDto>()
            .ForMember(dest => dest.ConsumerType, opt => opt.MapFrom(src => src.ConsumerTypeId));

        CreateMap<List<DialogContent>?, ContentDto?>()
            .ConvertUsing<DialogContentOutputConverter<ContentDto>>();

        CreateMap<DialogTransmissionSenderActor, DialogTransmissionSenderActorDto>()
            .ForMember(dest => dest.ActorType, opt => opt.MapFrom(src => src.ActorTypeId))
            .ForMember(dest => dest.ActorId, opt => opt.MapFrom(src => IdentifierMasker.GetMaybeMaskedIdentifier(src.ActorId)));

        CreateMap<List<DialogTransmissionContent>?, DialogTransmissionContentDto?>()
            .ConvertUsing<TransmissionContentOutputConverter<DialogTransmissionContentDto>>();

        CreateMap<DialogTransmission, DialogTransmissionDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.TypeId));

        CreateMap<DialogTransmissionAttachment, DialogTransmissionAttachmentDto>();
        CreateMap<AttachmentUrl, DialogTransmissionAttachmentUrlDto>()
            .ForMember(dest => dest.ConsumerType, opt => opt.MapFrom(src => src.ConsumerTypeId));
    }

}
