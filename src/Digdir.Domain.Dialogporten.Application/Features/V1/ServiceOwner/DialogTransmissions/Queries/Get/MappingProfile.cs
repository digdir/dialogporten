using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Content;
using Digdir.Domain.Dialogporten.Domain.Attachments;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions.Contents;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogTransmissions.Queries.Get;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DialogTransmission, GetDialogTransmissionDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.TypeId))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.Dialog.DeletedAt));

        CreateMap<DialogTransmissionSenderActor, GetDialogTransmissionSenderActorDto>()
            .ForMember(dest => dest.ActorType, opt => opt.MapFrom(src => src.ActorTypeId));

        CreateMap<List<DialogTransmissionContent>?, GetDialogTransmissionContentDto?>()
            .ConvertUsing<TransmissionContentOutputConverter<GetDialogTransmissionContentDto>>();

        CreateMap<DialogTransmissionAttachment, GetDialogTransmissionAttachmentDto>();
        CreateMap<AttachmentUrl, GetDialogTransmissionAttachmentUrlDto>()
            .ForMember(dest => dest.ConsumerType, opt => opt.MapFrom(src => src.ConsumerTypeId));
    }
}
