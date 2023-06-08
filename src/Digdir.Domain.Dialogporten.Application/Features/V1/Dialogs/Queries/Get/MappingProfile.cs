using AutoMapper;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements.DialogElementUrls;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Queries.Get;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DialogEntity, GetDialogDto>();
        CreateMap<DialogElement, GetDialogDialogElementDto>();
        CreateMap<DialogElementUrl, GetDialogDialogElementUrlDto>();
        CreateMap<DialogGuiAction, GetDialogDialogGuiActionDto>();
        CreateMap<DialogApiAction, GetDialogDialogApiActionDto>();
        CreateMap<DialogActivity, GetDialogDialogActivityDto>();
    }
}
