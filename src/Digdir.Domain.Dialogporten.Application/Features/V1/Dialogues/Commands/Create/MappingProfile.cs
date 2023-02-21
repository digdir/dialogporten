using AutoMapper;
using Digdir.Domain.Dialogporten.Domain.Dialogues;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Attachments;

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

