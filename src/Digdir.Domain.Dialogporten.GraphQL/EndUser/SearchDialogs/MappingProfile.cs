using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.Pagination;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Search;
using Digdir.Domain.Dialogporten.GraphQL.EndUser.Common;

namespace Digdir.Domain.Dialogporten.GraphQL.EndUser.SearchDialogs;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<SearchDialogInput, SearchDialogQuery>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

        CreateMap<PaginatedList<SearchDialogDto>, SearchDialogsPayload>();

        CreateMap<SearchDialogDto, SearchDialog>();
        // CreateMap<GetDialogDto, DialogSearch>()
        //     .ForMember(dest => dest.StatusSearch, opt => opt.MapFrom(src => src.Status));
        //
        // CreateMap<GetDialogContentDto, ContentSearch>()
        //     .ForMember(dest => dest.TypeSearch, opt => opt.MapFrom(src => src.Type));
        //
        // CreateMap<GetDialogDialogElementDto, ElementSearch>();
        // CreateMap<GetDialogDialogElementUrlDto, ElementUrlSearch>()
        //     .ForMember(dest => dest.ConsumerSearchType, opt => opt.MapFrom(src => src.ConsumerType));
        //
        // CreateMap<GetDialogDialogGuiActionDto, GuiActionSearch>()
        //     .ForMember(dest => dest.PrioritySearch, opt => opt.MapFrom(src => src.Priority));
        //
        // CreateMap<GetDialogDialogApiActionDto, ApiActionSearch>();
        // CreateMap<GetDialogDialogApiActionEndpointDto, ApiActionEndpointSearch>()
        //     .ForMember(dest => dest.HttpMethod, opt => opt.MapFrom(src => src.HttpMethod));
        //
        // CreateMap<GetDialogDialogActivityDto, ActivitySearch>()
        //     .ForMember(dest => dest.TypeSearch, opt => opt.MapFrom(src => src.Type));
        //
        // CreateMap<GetDialogDialogSeenLogDto, SeenLogSearch>();
    }
}
