using AutoMapper;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogActivities.Queries.Search;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DialogActivity, SearchDialogActivityDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.TypeId));
    }
}
