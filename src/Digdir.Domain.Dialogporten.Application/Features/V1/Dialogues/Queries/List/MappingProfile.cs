using AutoMapper;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Queries.List;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DialogueEntity, ListDialogueDto>();
    }
}