using System.Threading;
using AutoMapper;
using Digdir.Domain.Dialogporten.Application.Common.Pagination;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using MediatR;
using Yarp.ReverseProxy.Utilities;
using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.GetQueryable;
using HotChocolate.Authorization;

namespace Digdir.Domain.Dialogporten.GraphQL;

[Authorize]
public class DialogQueries
{
    //[UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IQueryable<GetQueryableDialogDto>> GetDialogs(
        [Service] ISender mediator,
        GetDialogQuery? query)
    {
        return await mediator.Send(query ?? new());
    }
}

//public class MappingProfile : Profile
//{
//    public MappingProfile()
//    {
//        CreateMap<GetDialogDto, Dialog>()
//            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

//        CreateMap<GetDialogContentDto, Content>()
//            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type));

//        CreateMap<LocalizationDto, Localization>();
//    }
//}

//public enum DialogStatus
//{
//    New = 1,

//    /// <summary>
//    /// Under arbeid. Generell status som brukes for dialogtjenester der ytterligere bruker-input er
//    /// forventet.
//    /// </summary>
//    InProgress = 2,

//    /// <summary>
//    /// Venter på tilbakemelding fra tjenesteeier
//    /// </summary>
//    Waiting = 3,

//    /// <summary>
//    /// Dialogen er i en tilstand hvor den venter på signering. Typisk siste steg etter at all
//    /// utfylling er gjennomført og validert.
//    /// </summary>
//    Signing = 4,

//    /// <summary>
//    /// Dialogen ble avbrutt. Dette gjør at dialogen typisk fjernes fra normale GUI-visninger.
//    /// </summary>
//    Cancelled = 5,

//    /// <summary>
//    /// Dialigen ble fullført. Dette gjør at dialogen typisk flyttes til et GUI-arkiv eller lignende.
//    /// </summary>
//    Completed = 6
//}

//public class Dialog
//{
//    public Guid Id { get; set; }
//    public Guid Revision { get; set; }
//    public string Org { get; set; } = null!;
//    public string ServiceResource { get; set; } = null!;
//    public string Party { get; set; } = null!;
//    public int? Progress { get; set; }
//    public string? ExtendedStatus { get; set; }
//    public string? ExternalReference { get; set; }
//    public DateTimeOffset? VisibleFrom { get; set; }
//    public DateTimeOffset? DueAt { get; set; }
//    public DateTimeOffset? ExpiresAt { get; set; }
//    public DateTimeOffset CreatedAt { get; set; }
//    public DateTimeOffset UpdatedAt { get; set; }

//    public DialogStatus Status { get; set; }

//    public List<Content> Content { get; set; } = [];
//    // public List<Element> Elements { get; set; } = [];
//    // public List<GuiAction> GuiActions { get; set; } = [];
//    // public List<ApiAction> ApiActions { get; set; } = [];
//    // public List<Activity> Activities { get; set; } = [];
//}

//public enum ContentType
//{
//    Title = 1,
//    SenderName = 2,
//    Summary = 3,
//    AdditionalInfo = 4
//}

//public sealed class Content
//{
//    public ContentType Type { get; set; }
//    public List<Localization> Value { get; set; } = [];
//}

//public sealed class Localization
//{
//    public string Value { get; set; } = null!;
//    public string CultureCode { get; set; } = null!;
//}
