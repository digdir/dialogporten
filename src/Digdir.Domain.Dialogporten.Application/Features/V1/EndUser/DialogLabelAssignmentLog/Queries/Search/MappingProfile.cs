using AutoMapper;
using Digdir.Domain.Dialogporten.Domain.DialogEndUserContexts.Entities;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogLabelAssignmentLog.Queries.Search;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<LabelAssignmentLog, LabelAssignmentLogDto>();
        CreateMap<LabelAssignmentLogActor, LabelAssignmentLogActorDto>();
    }
}
