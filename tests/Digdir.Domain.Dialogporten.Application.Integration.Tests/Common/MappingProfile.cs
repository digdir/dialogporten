using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Create;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Update;

namespace Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<GetDialogDto, UpdateDialogDto>()
            .ForMember(dest => dest.Activities, opt => opt.Ignore());
        CreateMap<GetDialogContentDto, UpdateDialogContentDto>();
        CreateMap<GetDialogSearchTagDto, UpdateDialogSearchTagDto>();
        CreateMap<GetDialogDialogElementDto, UpdateDialogDialogElementDto>();
        CreateMap<GetDialogDialogGuiActionDto, UpdateDialogDialogGuiActionDto>();
        CreateMap<GetDialogDialogApiActionDto, UpdateDialogDialogApiActionDto>();
        CreateMap<GetDialogDialogElementUrlDto, UpdateDialogDialogElementUrlDto>();
        CreateMap<GetDialogDialogApiActionEndpointDto, UpdateDialogDialogApiActionEndpointDto>();

    }
}
