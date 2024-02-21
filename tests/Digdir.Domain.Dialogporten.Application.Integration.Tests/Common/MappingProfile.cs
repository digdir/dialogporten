using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Create;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Update;

namespace Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateDialogCommand, UpdateDialogDto>()
            .ForMember(dest => dest.Activities, opt => opt.Ignore());
        CreateMap<CreateDialogContentDto, UpdateDialogContentDto>();
        CreateMap<CreateDialogSearchTagDto, UpdateDialogSearchTagDto>();
        CreateMap<CreateDialogDialogElementDto, UpdateDialogDialogElementDto>();
        CreateMap<CreateDialogDialogGuiActionDto, UpdateDialogDialogGuiActionDto>();
        CreateMap<CreateDialogDialogApiActionDto, UpdateDialogDialogApiActionDto>();
        CreateMap<CreateDialogDialogElementUrlDto, UpdateDialogDialogElementUrlDto>();
        CreateMap<CreateDialogDialogApiActionEndpointDto, UpdateDialogDialogApiActionEndpointDto>();

    }
}
