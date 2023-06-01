using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.Attachments;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.TokenScopes;

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
        CreateMap<UpdateDialogueDialogueApiActionDto, DialogueApiAction>()
            .IgnoreComplexDestinationProperties();
        CreateMap<UpdateDialogueDialogueApiActionDto, DialogueApiAction>()
            .IgnoreComplexDestinationProperties();
        CreateMap<UpdateDialogueDialogueGuiActionDto, DialogueGuiAction>()
            .IgnoreComplexDestinationProperties();
        CreateMap<UpdateDialogueDialogueAttachmentDto, DialogueAttachement>()
            .IgnoreComplexDestinationProperties();

        // Since this is append only, we don't need to merge with existing
        // history records and thus can map complex properties
        CreateMap<UpdateDialogueDialogueActivityDto, DialogueActivity>();

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