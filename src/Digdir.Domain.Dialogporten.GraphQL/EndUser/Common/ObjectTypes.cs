namespace Digdir.Domain.Dialogporten.GraphQL.EndUser.Common;

public sealed class Localization
{
    public string Value { get; set; } = null!;
    public string CultureCode { get; set; } = null!;
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
