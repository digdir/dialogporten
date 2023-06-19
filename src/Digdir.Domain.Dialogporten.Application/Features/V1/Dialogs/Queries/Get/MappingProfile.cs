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
        // Since only one or the other of GUI or API actions are required, the DTOs are nullable
        // so that the null value can be returned if the list is empty, causing the JSON serializer
        // to ignore the property.
        CreateMap<DialogEntity, GetDialogDto>();
            /* FIXME! This causes EF to complain for some reason.
            .ForMember(
                dest => dest.GuiActions,
                opt => opt.MapFrom(src => src.GuiActions.Any() ? src.GuiActions : null))
            .ForMember(
                dest => dest.ApiActions,
                opt => opt.MapFrom(src => src.ApiActions.Any() ? src.ApiActions : null));
            */
        CreateMap<DialogGuiAction, GetDialogDialogGuiActionDto>();
        CreateMap<DialogApiAction, GetDialogDialogApiActionDto>();
        CreateMap<DialogApiActionEndpoint, GetDialogDialogApiActionEndpointDto>();
        CreateMap<DialogElement, GetDialogDialogElementDto>();
        CreateMap<DialogElementUrl, GetDialogDialogElementUrlDto>();
        CreateMap<DialogActivity, GetDialogDialogActivityDto>();

    }
}
