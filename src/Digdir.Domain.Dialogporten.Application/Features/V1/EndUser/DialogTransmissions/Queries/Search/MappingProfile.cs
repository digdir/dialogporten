using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Content;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actors;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Attachments;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Contents;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogTransmissions.Queries.Search;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DialogTransmission, SearchDialogTransmissionDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.TypeId))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.Dialog.DeletedAt));

        CreateMap<DialogActor, SearchDialogTransmissionActorDto>()
            .ForMember(dest => dest.ActorType, opt => opt.MapFrom(src => src.ActorTypeId));

        CreateMap<List<TransmissionContent>?, SearchDialogTransmissionContentDto?>()
            .ConvertUsing<TransmissionContentOutputConverter<SearchDialogTransmissionContentDto>>();

        CreateMap<TransmissionAttachment, SearchDialogTransmissionAttachmentDto>();
        CreateMap<AttachmentUrl, SearchDialogTransmissionAttachmentUrlDto>()
            .ForMember(dest => dest.ConsumerType, opt => opt.MapFrom(src => src.ConsumerTypeId));
    }
}
