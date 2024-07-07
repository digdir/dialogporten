using AutoMapper;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Content;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.Content;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // See IntermediateDialogContent
        CreateMap<IntermediateDialogContent, DialogContent>();
        CreateMap<DialogContent, DialogContentValueDto>();
    }
}
