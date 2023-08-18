using AutoMapper;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.DialogActivities.Queries.EndUser.List;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DialogActivity, ListDialogActivityDto>()
            .ForMember(dest => dest.Type,
                opt => opt.MapFrom(src => src.TypeId));
    }
}