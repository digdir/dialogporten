using AutoMapper;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Content;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Search;

internal sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DialogEntity, SearchDialogDto>()
            .ForMember(dest => dest.LatestActivities, opt => opt.Ignore())
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content.Where(x => x.Type.OutputInList)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.StatusId));

        CreateMap<DialogContent, SearchDialogContentDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.TypeId));

        CreateMap<DialogActivity, SearchDialogDialogActivityDto>()
            .ForMember(dest => dest.SeenByEndUserIdHash, opt => opt.MapFrom(src => src.SeenByEndUserId))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.TypeId));
    }
}
