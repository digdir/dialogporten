using System.Reflection.Emit;
using AutoMapper;
using Digdir.Domain.Dialogporten.Domain.DialogEndUserContexts.Entities;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogLabelAssigmentLog.Queries.Search;

public sealed class MappingProfile : Profile
{

    public MappingProfile()
    {
        CreateMap<LabelAssignmentLog, SearchDialogLabelAssignmentLogDto>();
        CreateMap<LabelAssignmentLogActor, LabelAssignmentLogActorDto>();
    }
}
