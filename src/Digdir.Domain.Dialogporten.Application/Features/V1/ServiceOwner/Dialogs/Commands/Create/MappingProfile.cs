using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Content;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actors;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Attachments;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Contents;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Create;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateDialogDto, DialogEntity>()
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.Status));
        CreateMap<CreateDialogSearchTagDto, DialogSearchTag>();

        CreateMap<CreateDialogDialogAttachmentDto, DialogAttachment>();
        CreateMap<CreateDialogDialogAttachmentUrlDto, AttachmentUrl>()
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

        CreateMap<CreateDialogDialogActivityActorDto, DialogActor>()
            .ForMember(dest => dest.ActorType, opt => opt.Ignore())
            .ForMember(dest => dest.ActorTypeId, opt => opt.MapFrom(src => src.ActorType));

        CreateMap<CreateDialogContentDto?, List<DialogContent>?>()
            .ConvertUsing<DialogContentInputConverter<CreateDialogContentDto>>();

        CreateMap<CreateDialogDialogTransmissionContentDto?, List<TransmissionContent>?>()
            .ConvertUsing<TransmissionContentInputConverter<CreateDialogDialogTransmissionContentDto>>();

        CreateMap<CreateDialogDialogTransmissionActorDto, DialogActor>()
            .ForMember(dest => dest.ActorType, opt => opt.Ignore())
            .ForMember(dest => dest.ActorTypeId, opt => opt.MapFrom(src => src.ActorType));

        CreateMap<CreateDialogDialogTransmissionDto, DialogTransmission>()
            .ForMember(dest => dest.Type, opt => opt.Ignore())
            .ForMember(dest => dest.TypeId, opt => opt.MapFrom(src => src.Type));

        CreateMap<CreateDialogTransmissionAttachmentDto, TransmissionAttachment>();
        CreateMap<CreateDialogTransmissionAttachmentUrlDto, AttachmentUrl>()
            .ForMember(dest => dest.ConsumerType, opt => opt.Ignore())
            .ForMember(dest => dest.ConsumerTypeId, opt => opt.MapFrom(src => src.ConsumerType));
    }
}
