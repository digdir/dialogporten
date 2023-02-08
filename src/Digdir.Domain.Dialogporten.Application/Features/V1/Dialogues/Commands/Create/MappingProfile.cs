using AutoMapper;
using Digdir.Domain.Dialogporten.Domain;
using System.Security.Cryptography.X509Certificates;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Commands.Create;

internal sealed class MappingProfile : Profile
{
	public MappingProfile()
	{
		CreateMap<CreateDialogueDto, DialogueEntity>()
			.ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title));
	}
}

