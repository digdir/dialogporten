using AutoMapper;
using Digdir.Domain.Dialogporten.Domain;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Queries.Get;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DialogueEntity, GetDialogueDto>();
    }
}