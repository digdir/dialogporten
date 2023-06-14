using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Commands.Update;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        // In
        CreateMap<UpdateDialogDto, DialogEntity>()
            .IgnoreComplexDestinationProperties();
        CreateMap<UpdateDialogDialogApiActionDto, DialogApiAction>()
            .IgnoreComplexDestinationProperties();
        CreateMap<UpdateDialogDialogApiActionDto, DialogApiAction>()
            .IgnoreComplexDestinationProperties();
        CreateMap<UpdateDialogDialogGuiActionDto, DialogGuiAction>()
            .IgnoreComplexDestinationProperties();
        CreateMap<UpdateDialogDialogElementDto, DialogElement>()
            .IgnoreComplexDestinationProperties();

        // Since this is append only, we don't need to merge with existing
        // history records and thus can map complex properties
        CreateMap<UpdateDialogDialogActivityDto, DialogActivity>();

        // To support json patch
        CreateMap<DialogEntity, UpdateDialogDto>();
        CreateMap<DialogActivity, UpdateDialogDialogActivityDto>();
        CreateMap<DialogApiAction, UpdateDialogDialogApiActionDto>();
        CreateMap<DialogApiAction, UpdateDialogDialogApiActionDto>();
        CreateMap<DialogGuiAction, UpdateDialogDialogGuiActionDto>();
        CreateMap<DialogElement, UpdateDialogDialogElementDto>();
        CreateMap<DialogElementUrl, UpdateDialogDialogElementUrlDto>();
    }
}
