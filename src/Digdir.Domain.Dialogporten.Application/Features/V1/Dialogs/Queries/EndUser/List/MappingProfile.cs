using AutoMapper;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Queries.EndUser.List;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DialogEntity, ListDialogDto>()
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.StatusId));
        CreateMap<DialogActivity, ListDialogDialogActivityDto>()
            .ForMember(dest => dest.Type,
                opt => opt.MapFrom(src => src.TypeId));
    }
}