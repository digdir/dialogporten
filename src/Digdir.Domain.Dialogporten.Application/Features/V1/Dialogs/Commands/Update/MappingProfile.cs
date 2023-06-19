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
        CreateMap<UpdateDialogDialogApiActionEndpointDto, DialogApiActionEndpoint>()
            .IgnoreComplexDestinationProperties();
        CreateMap<UpdateDialogDialogElementDto, DialogElement>()
            .IgnoreComplexDestinationProperties();
        CreateMap<UpdateDialogDialogElementUrlDto, DialogElementUrl>()
            .IgnoreComplexDestinationProperties();

        // Since this is append only, we don't need to merge with existing
        // activity records and thus can map complex properties
        CreateMap<UpdateDialogDialogActivityDto, DialogActivity>();

        // To support json patch
        CreateMap<DialogEntity, UpdateDialogDto>();
        CreateMap<DialogActivity, UpdateDialogDialogActivityDto>();
        CreateMap<DialogApiAction, UpdateDialogDialogApiActionDto>();
        CreateMap<DialogApiActionEndpoint, UpdateDialogDialogApiActionEndpointDto>();
        CreateMap<DialogGuiAction, UpdateDialogDialogGuiActionDto>();
        CreateMap<DialogElement, UpdateDialogDialogElementDto>();
        CreateMap<DialogElementUrl, UpdateDialogDialogElementUrlDto>();
    }
}
