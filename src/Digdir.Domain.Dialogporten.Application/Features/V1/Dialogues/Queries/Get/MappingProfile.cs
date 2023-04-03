using AutoMapper;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.Attachments;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.TokenScopes;

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