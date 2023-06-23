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
        // TODO: should we ignore id for entities where user cannot set id on create?

        // In
        CreateMap<UpdateDialogDto, DialogEntity>()
            .IgnoreComplexDestinationProperties();

        CreateMap<UpdateDialogDialogApiActionDto, DialogApiAction>()
            .IgnoreComplexDestinationProperties()
            .ForMember(x => x.Id, opt => opt.Ignore());
        CreateMap<UpdateDialogDialogApiActionEndpointDto, DialogApiActionEndpoint>()
            .IgnoreComplexDestinationProperties()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(dest => dest.HttpMethodId, opt => opt.MapFrom(src => src.HttpMethod));

        CreateMap<UpdateDialogDialogGuiActionDto, DialogGuiAction>()
            .IgnoreComplexDestinationProperties()
            .ForMember(x => x.Id, opt => opt.Ignore());

        CreateMap<UpdateDialogDialogElementDto, DialogElement>()
            .IgnoreComplexDestinationProperties();
        CreateMap<UpdateDialogDialogElementUrlDto, DialogElementUrl>()
            .IgnoreComplexDestinationProperties()
            .ForMember(x => x.Id, opt => opt.Ignore());

        // Since this is append only, we don't need to merge with existing
        // activity records and thus can map complex properties
        CreateMap<UpdateDialogDialogActivityDto, DialogActivity>();

        // To support json patch
        CreateMap<DialogEntity, UpdateDialogDto>();
        CreateMap<DialogActivity, UpdateDialogDialogActivityDto>();
        CreateMap<DialogApiAction, UpdateDialogDialogApiActionDto>();
        CreateMap<DialogApiActionEndpoint, UpdateDialogDialogApiActionEndpointDto>()
            .ForMember(dest => dest.HttpMethod, opt => opt.MapFrom(src => src.HttpMethodId));
        CreateMap<DialogGuiAction, UpdateDialogDialogGuiActionDto>();
        CreateMap<DialogElement, UpdateDialogDialogElementDto>();
        CreateMap<DialogElementUrl, UpdateDialogDialogElementUrlDto>();
    }
}
