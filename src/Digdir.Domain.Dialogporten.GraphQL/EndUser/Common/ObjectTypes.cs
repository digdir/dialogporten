namespace Digdir.Domain.Dialogporten.GraphQL.EndUser.Common;

public sealed class Localization
{
    public string Value { get; set; } = null!;
    public string LanguageCode { get; set; } = null!;
}

public sealed class ContentValue
{
    public List<Localization> Value { get; set; } = [];
    public string MediaType { get; set; } = null!;
}

public sealed class SeenLog
{
    public Guid Id { get; set; }
    public DateTimeOffset SeenAt { get; set; }

    public Actor SeenBy { get; set; } = null!;

    public bool IsCurrentEndUser { get; set; }
}

public sealed class Activity
{
    public Guid Id { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public Uri? ExtendedType { get; set; }

    public ActivityType Type { get; set; }

    public Guid? RelatedActivityId { get; set; }

    public Actor PerformedBy { get; set; } = null!;

    public List<Localization> Description { get; set; } = [];
}

public sealed class Actor
{
    public ActorType? ActorType { get; set; }
    public string? ActorId { get; set; }
    public string? ActorName { get; set; }
}

public enum ActorType
{
    PartyRepresentative = 1,
    ServiceOwner = 2
}

public enum ActivityType
{
    [GraphQLDescription("Refers to a dialog that has been created.")]
    DialogCreated = 1,

    [GraphQLDescription("Refers to a dialog that has been completed.")]
    DialogCompleted = 2,

    [GraphQLDescription("Refers to a dialog that has been closed.")]
    DialogClosed = 3,

    [GraphQLDescription("Information from the service provider, not (directly) related to any transmission.")]
    Information = 4,

    [GraphQLDescription("Refers to a transmission that has been opened.")]
    TransmissionOpened = 5,

    [GraphQLDescription("Indicates that payment has been made.")]
    PaymentMade = 6,

    [GraphQLDescription("Indicates that a signature has been provided.")]
    SignatureProvided = 7
}

public enum DialogStatus
{
    [GraphQLDescription("The dialogue is considered new. Typically used for simple messages that do not require any interaction, or as an initial step for dialogues. This is the default.")]
    New = 1,

    [GraphQLDescription("Started. In a serial process, this is used to indicate that, for example, a form filling is ongoing.")]
    InProgress = 2,

    [GraphQLDescription("Equivalent to \"InProgress\", but will be used by the workspace/frontend for display purposes.")]
    Signing = 3,

    [GraphQLDescription("For processing by the service owner. In a serial process, this is used after a submission is made.")]
    Processing = 4,

    [GraphQLDescription("Used to indicate that the dialogue is in progress/under work, but is in a state where the user must do something - for example, correct an error, or other conditions that hinder further processing.")]
    RequiresAttention = 5,

    [GraphQLDescription("The dialogue was completed. This typically means that the dialogue is moved to a GUI archive or similar.")]
    Completed = 6
}
