using AutoMapper;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Commands.Create;

internal sealed class MappingProfile : Profile
{
	public MappingProfile()
	{
		CreateMap<CreateDialogDto, DialogEntity>();
		CreateMap<CreateDialogDialogElementDto, DialogElement>();
        CreateMap<CreateDialogDialogElementUrlDto, DialogElementUrl>();
		CreateMap<CreateDialogDialogGuiActionDto, DialogGuiAction>();
		CreateMap<CreateDialogDialogApiActionDto, DialogApiAction>();
        CreateMap<CreateDialogDialogApiActionEndpointDto, DialogApiActionEndpoint>()
			.ForMember(dest => dest.HttpMethodId, opt => opt.MapFrom(src => src.HttpMethod));
		CreateMap<CreateDialogDialogActivityDto, DialogActivity>();
	}
}

