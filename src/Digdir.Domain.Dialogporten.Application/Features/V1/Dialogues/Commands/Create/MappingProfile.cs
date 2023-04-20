using AutoMapper;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.Attachments;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.TokenScopes;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Commands.Create;

internal sealed class MappingProfile : Profile
{
	public MappingProfile()
	{
		CreateMap<CreateDialogueDto, DialogueEntity>();
		CreateMap<CreateDialogueDialogueAttachmentDto, DialogueAttachement>();
		CreateMap<CreateDialogueDialogueGuiActionDto, DialogueGuiAction>();
		CreateMap<CreateDialogueDialogueApiActionDto, DialogueApiAction>();
		CreateMap<CreateDialogueDialogueActivityDto, DialogueActivity>();
		CreateMap<CreateDialogueDialogueTokenScopeDto, DialogueTokenScope>();
	}
}

