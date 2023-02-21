using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Domain.Dialogues;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Attachments;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Commands.Update;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<UpdateDialogueDto, DialogueEntity>()
            .IgnoreComplexDestinationProperties();
        CreateMap<UpdateDialogueDialogueTokenScopeDto, DialogueTokenScope>();
        CreateMap<UpdateDialogueDialogueActivityDto, DialogueActivity>();
        CreateMap<UpdateDialogueDialogueApiActionDto, DialogueApiAction>();
        CreateMap<UpdateDialogueDialogueApiActionDto, DialogueApiAction>();
        CreateMap<UpdateDialogueDialogueGuiActionDto, DialogueGuiAction>()
            .IgnoreComplexDestinationProperties();

        CreateMap<UpdateDialogueDialogueAttachmentDto, DialogueAttachement>()
            .IgnoreComplexDestinationProperties();
    }
}