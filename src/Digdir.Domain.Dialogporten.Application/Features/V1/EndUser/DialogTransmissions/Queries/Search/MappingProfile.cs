using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Content;
using Digdir.Domain.Dialogporten.Domain.Attachments;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions.Contents;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogTransmissions.Queries.Search;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DialogTransmission, SearchDialogTransmissionDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.TypeId))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.Dialog.DeletedAt));

        CreateMap<DialogTransmissionSenderActor, SearchDialogTransmissionSenderActorDto>()
            .ForMember(dest => dest.ActorType, opt => opt.MapFrom(src => src.ActorTypeId));

        CreateMap<List<DialogTransmissionContent>?, SearchDialogTransmissionContentDto?>()
            .ConvertUsing<TransmissionContentOutputConverter<SearchDialogTransmissionContentDto>>();

        CreateMap<DialogTransmissionAttachment, SearchDialogTransmissionAttachmentDto>();
        CreateMap<AttachmentUrl, SearchDialogTransmissionAttachmentUrlDto>()
            .ForMember(dest => dest.ConsumerType, opt => opt.MapFrom(src => src.ConsumerTypeId));
    }
}
