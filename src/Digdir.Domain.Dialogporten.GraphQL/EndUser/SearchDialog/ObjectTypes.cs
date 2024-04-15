using Digdir.Domain.Dialogporten.GraphQL.EndUser.Common;

namespace Digdir.Domain.Dialogporten.GraphQL.EndUser.SearchDialog;

public enum DialogStatusSearch
{
    New = 1,

    /// <summary>
    /// Under arbeid. Generell status som brukes for dialogtjenester der ytterligere bruker-input er
    /// forventet.
    /// </summary>
    InProgress = 2,

    /// <summary>
    /// Venter på tilbakemelding fra tjenesteeier
    /// </summary>
    Waiting = 3,

    /// <summary>
    /// Dialogen er i en tilstand hvor den venter på signering. Typisk siste steg etter at all
    /// utfylling er gjennomført og validert.
    /// </summary>
    Signing = 4,

    /// <summary>
    /// Dialogen ble avbrutt. Dette gjør at dialogen typisk fjernes fra normale GUI-visninger.
    /// </summary>
    Cancelled = 5,

    /// <summary>
    /// Dialigen ble fullført. Dette gjør at dialogen typisk flyttes til et GUI-arkiv eller lignende.
    /// </summary>
    Completed = 6
}

public sealed class DialogSearch
{
    public Guid Id { get; set; }
    public Guid Revision { get; set; }
    public string Org { get; set; } = null!;
    public string ServiceResource { get; set; } = null!;
    public string Party { get; set; } = null!;
    public int? Progress { get; set; }
    public string? ExtendedStatus { get; set; }
    public string? ExternalReference { get; set; }
    public DateTimeOffset? VisibleFrom { get; set; }
    public DateTimeOffset? DueAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public string? DialogToken { get; set; }

    public DialogStatusSearch StatusSearch { get; set; }

    public List<ContentSearch> Content { get; set; } = [];
    public List<ElementSearch> Elements { get; set; } = [];
    public List<GuiActionSearch> GuiActions { get; set; } = [];
    public List<ApiActionSearch> ApiActions { get; set; } = [];
    public List<ActivitySearch> Activities { get; set; } = [];
    public List<SeenLogSearch> SeenSinceLastUpdate { get; set; } = [];
}

public sealed class SeenLogSearch
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public string EndUserIdHash { get; set; } = null!;

    public string? EndUserName { get; set; }

    public bool IsCurrentEndUser { get; set; }
}

public sealed class ActivitySearch
{
    public Guid Id { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public Uri? ExtendedType { get; set; }

    public ActivityTypeSearch TypeSearch { get; set; }

    public Guid? RelatedActivityId { get; set; }
    public Guid? DialogElementId { get; set; }

    public List<Localization>? PerformedBy { get; set; } = [];
    public List<Localization> Description { get; set; } = [];
}

public enum ActivityTypeSearch
{
    /// <summary>
    /// Refererer en innsending utført av party som er mottatt hos tjenestetilbyder.
    /// </summary>
    Submission = 1,

    /// <summary>
    /// Indikerer en tilbakemelding fra tjenestetilbyder på en innsending. Inneholder
    /// referanse til den aktuelle innsendingen.
    /// </summary>
    Feedback = 2,

    /// <summary>
    /// Informasjon fra tjenestetilbyder, ikke (direkte) relatert til noen innsending.
    /// </summary>
    Information = 3,

    /// <summary>
    /// Brukes for å indikere en feilsituasjon, typisk på en innsending. Inneholder en
    /// tjenestespesifikk activityErrorCode.
    /// </summary>
    Error = 4,

    /// <summary>
    /// Indikerer at dialogen er lukket for videre endring. Dette skjer typisk ved fullføring
    /// av dialogen, eller sletting.
    /// </summary>
    Closed = 5,

    /// <summary>
    /// Når dialogen blir videresendt (tilgang delegert) av noen med tilgang til andre.
    /// </summary>
    Forwarded = 7
}

public sealed class ApiActionSearch
{
    public Guid Id { get; set; }
    public string Action { get; set; } = null!;
    public string? AuthorizationAttribute { get; set; }
    public bool IsAuthorized { get; set; }

    public Guid? DialogElementId { get; set; }

    public List<ApiActionEndpointSearch> Endpoints { get; set; } = [];
}

// ReSharper disable InconsistentNaming
public enum HttpVerbSearch
{
    GET = 1,
    POST = 2,
    PUT = 3,
    PATCH = 4,
    DELETE = 5,
    HEAD = 6,
    OPTIONS = 7,
    TRACE = 8,
    CONNECT = 9
}

public sealed class ApiActionEndpointSearch
{
    public Guid Id { get; set; }
    public string? Version { get; set; }
    public Uri Url { get; set; } = null!;
    public HttpVerbSearch HttpMethod { get; set; }
    public Uri? DocumentationUrl { get; set; }
    public Uri? RequestSchema { get; set; }
    public Uri? ResponseSchema { get; set; }
    public bool Deprecated { get; set; }
    public DateTimeOffset? SunsetAt { get; set; }
}

public sealed class GuiActionSearch
{
    public Guid Id { get; set; }
    public string Action { get; set; } = null!;
    public Uri Url { get; set; } = null!;
    public string? AuthorizationAttribute { get; set; }
    public bool IsAuthorized { get; set; }
    public bool IsBackChannel { get; set; }
    public bool IsDeleteAction { get; set; }

    public GuiActionPrioritySearch PrioritySearch { get; set; }

    public List<Localization> Title { get; set; } = [];
}

public enum GuiActionPrioritySearch
{
    Primary = 1,
    Secondary = 2,
    Tertiary = 3
}

public sealed class ElementSearch
{
    public Guid Id { get; set; }
    public Uri? Type { get; set; }
    public string? ExternalReference { get; set; }
    public string? AuthorizationAttribute { get; set; }
    public bool IsAuthorized { get; set; }

    public Guid? RelatedDialogElementId { get; set; }

    public List<Localization> DisplayName { get; set; } = [];
    public List<ElementUrlSearch> Urls { get; set; } = [];
}

public sealed class ElementUrlSearch
{
    public Guid Id { get; set; }
    public Uri Url { get; set; } = null!;
    public string? MimeType { get; set; }

    public ElementUrlConsumerSearch ConsumerSearchType { get; set; }
}

public enum ElementUrlConsumerSearch
{
    Gui = 1,
    Api = 2
}
public enum ContentTypeSearch
{
    Title = 1,
    SenderName = 2,
    Summary = 3,
    AdditionalInfo = 4
}

public sealed class ContentSearch
{
    public ContentTypeSearch TypeSearch { get; set; }
    public List<Localization> Value { get; set; } = [];
}

// public sealed class Localization
// {
//     public string Value { get; set; } = null!;
//     public string CultureCode { get; set; } = null!;
// }
