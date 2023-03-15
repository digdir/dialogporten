using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Domain.Dialogues;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Attachments;
using Digdir.Domain.Dialogporten.Domain.Dialogues.TokenScopes;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Commands.Update;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        // In
        CreateMap<UpdateDialogueDto, DialogueEntity>()
            .IgnoreComplexDestinationProperties();
        CreateMap<UpdateDialogueDialogueTokenScopeDto, DialogueTokenScope>()
            .IgnoreComplexDestinationProperties();
        CreateMap<UpdateDialogueDialogueActivityDto, DialogueActivity>()
            .IgnoreComplexDestinationProperties();
        CreateMap<UpdateDialogueDialogueApiActionDto, DialogueApiAction>()
            .IgnoreComplexDestinationProperties();
        CreateMap<UpdateDialogueDialogueApiActionDto, DialogueApiAction>()
            .IgnoreComplexDestinationProperties();
        CreateMap<UpdateDialogueDialogueGuiActionDto, DialogueGuiAction>()
            .IgnoreComplexDestinationProperties();
        CreateMap<UpdateDialogueDialogueAttachmentDto, DialogueAttachement>()
            .IgnoreComplexDestinationProperties();

        // To support json patch
        CreateMap<DialogueEntity, UpdateDialogueDto>();
        CreateMap<DialogueTokenScope, UpdateDialogueDialogueTokenScopeDto>();
        CreateMap<DialogueActivity, UpdateDialogueDialogueActivityDto>();
        CreateMap<DialogueApiAction, UpdateDialogueDialogueApiActionDto>();
        CreateMap<DialogueApiAction, UpdateDialogueDialogueApiActionDto>();
        CreateMap<DialogueGuiAction, UpdateDialogueDialogueGuiActionDto>();
        CreateMap<DialogueAttachement, UpdateDialogueDialogueAttachmentDto>();
    }
}