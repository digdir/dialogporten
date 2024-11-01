using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.Pagination;
using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Search;

namespace Digdir.Domain.Dialogporten.GraphQL.EndUser.SearchDialogs;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<SearchDialogInput, SearchDialogQuery>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

        CreateMap<PaginatedList<DialogDto>, SearchDialogsPayload>();

        CreateMap<ContentDto, SearchContent>();

        CreateMap<DialogDto, SearchDialog>();
    }
}
