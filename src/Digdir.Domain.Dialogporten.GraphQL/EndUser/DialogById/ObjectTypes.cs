using Digdir.Domain.Dialogporten.GraphQL.EndUser.Common;

namespace Digdir.Domain.Dialogporten.GraphQL.EndUser.DialogById;

public enum DialogStatus
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

public sealed class Dialog
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

    public DialogStatus Status { get; set; }

    public List<Content> Content { get; set; } = [];
    public List<Element> Elements { get; set; } = [];
    public List<GuiAction> GuiActions { get; set; } = [];
    public List<ApiAction> ApiActions { get; set; } = [];
    public List<Activity> Activities { get; set; } = [];
    public List<SeenLog> SeenSinceLastUpdate { get; set; } = [];
}

public sealed class SeenLog
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public string EndUserIdHash { get; set; } = null!;

    public string? EndUserName { get; set; }

    public bool IsCurrentEndUser { get; set; }
}

public sealed class Activity
{
    public Guid Id { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public Uri? ExtendedType { get; set; }

    public ActivityType Type { get; set; }

    public Guid? RelatedActivityId { get; set; }
    public Guid? DialogElementId { get; set; }

    public List<Localization>? PerformedBy { get; set; } = [];
    public List<Localization> Description { get; set; } = [];
}

public enum ActivityType
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

public sealed class ApiAction
{
    public Guid Id { get; set; }
    public string Action { get; set; } = null!;
    public string? AuthorizationAttribute { get; set; }
    public bool IsAuthorized { get; set; }

    public Guid? DialogElementId { get; set; }

    public List<ApiActionEndpoint> Endpoints { get; set; } = [];
}

// ReSharper disable InconsistentNaming
public enum HttpVerb
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

public sealed class ApiActionEndpoint
{
    public Guid Id { get; set; }
    public string? Version { get; set; }
    public Uri Url { get; set; } = null!;
    public HttpVerb HttpMethod { get; set; }
    public Uri? DocumentationUrl { get; set; }
    public Uri? RequestSchema { get; set; }
    public Uri? ResponseSchema { get; set; }
    public bool Deprecated { get; set; }
    public DateTimeOffset? SunsetAt { get; set; }
}

public sealed class GuiAction
{
    public Guid Id { get; set; }
    public string Action { get; set; } = null!;
    public Uri Url { get; set; } = null!;
    public string? AuthorizationAttribute { get; set; }
    public bool IsAuthorized { get; set; }
    public bool IsBackChannel { get; set; }
    public bool IsDeleteAction { get; set; }

    public GuiActionPriority Priority { get; set; }

    public List<Localization> Title { get; set; } = [];
}

public enum GuiActionPriority
{
    Primary = 1,
    Secondary = 2,
    Tertiary = 3
}

public sealed class Element
{
    public Guid Id { get; set; }
    public Uri? Type { get; set; }
    public string? ExternalReference { get; set; }
    public string? AuthorizationAttribute { get; set; }
    public bool IsAuthorized { get; set; }

    public Guid? RelatedDialogElementId { get; set; }

    public List<Localization> DisplayName { get; set; } = [];
    public List<ElementUrl> Urls { get; set; } = [];
}

public sealed class ElementUrl
{
    public Guid Id { get; set; }
    public Uri Url { get; set; } = null!;
    public string? MimeType { get; set; }

    public ElementUrlConsumer ConsumerType { get; set; }
}

public enum ElementUrlConsumer
{
    Gui = 1,
    Api = 2
}
public enum ContentType
{
    Title = 1,
    SenderName = 2,
    Summary = 3,
    AdditionalInfo = 4
}

public sealed class Content
{
    public ContentType Type { get; set; }
    public List<Localization> Value { get; set; } = [];
}
