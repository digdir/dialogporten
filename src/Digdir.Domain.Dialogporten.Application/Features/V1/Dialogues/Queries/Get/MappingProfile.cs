using AutoMapper;
using Digdir.Domain.Dialogporten.Domain.Dialogues;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Attachments;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Queries.Get;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DialogueEntity, GetDialogueDto>();
        CreateMap<DialogueAttachement, GetDialogueDialogueAttachmentDto>();
        CreateMap<DialogueGuiAction, GetDialogueDialogueGuiActionDto>();
        CreateMap<DialogueApiAction, GetDialogueDialogueApiActionDto>();
        CreateMap<DialogueActivity, GetDialogueDialogueActivityDto>();
        CreateMap<DialogueTokenScope, GetDialogueDialogueTokenScopeDto>();
    }
}