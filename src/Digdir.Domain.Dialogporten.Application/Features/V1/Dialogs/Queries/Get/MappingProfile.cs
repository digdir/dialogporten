using AutoMapper;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Queries.Get;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DialogEntity, GetDialogDto>();
        CreateMap<DialogGuiAction, GetDialogDialogGuiActionDto>();
        CreateMap<DialogApiAction, GetDialogDialogApiActionDto>();
        CreateMap<DialogApiActionEndpoint, GetDialogDialogApiActionEndpointDto>()
            .ForMember(dest => dest.HttpMethod, opt => opt.MapFrom(src => src.HttpMethodId));
        CreateMap<DialogElement, GetDialogDialogElementDto>();
        CreateMap<DialogElementUrl, GetDialogDialogElementUrlDto>();
        CreateMap<DialogActivity, GetDialogDialogActivityDto>();

    }
}
