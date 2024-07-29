using AutoMapper;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Contents;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Content;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // See IntermediateDialogContent
        CreateMap<IntermediateTransmissionContent, TransmissionContent>();
        CreateMap<IntermediateDialogContent, DialogContent>();
        CreateMap<TransmissionContent, ContentValueDto>();
        CreateMap<DialogContent, ContentValueDto>();
    }
}
