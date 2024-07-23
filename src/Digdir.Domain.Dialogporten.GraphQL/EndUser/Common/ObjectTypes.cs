namespace Digdir.Domain.Dialogporten.GraphQL.EndUser.Common;

public sealed class Localization
{
    public string Value { get; set; } = null!;
    public string LanguageCode { get; set; } = null!;
}

public enum ContentType
{
    Title = 1,
    SenderName = 2,
    Summary = 3,
    AdditionalInfo = 4,
    ExtendedStatus = 5,
    MainContentReference = 6,
}

public sealed class Content
{
    public ContentType Type { get; set; }
    public List<Localization> Value { get; set; } = [];
    public string? MediaType { get; set; }
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
    [GraphQLDescription("Refers to a submission made by a party that has been received by the service provider.")]
    Submission = 1,

    [GraphQLDescription("Indicates feedback from the service provider on a submission. Contains a reference to the current submission.")]
    Feedback = 2,

    [GraphQLDescription("Information from the service provider, not (directly) related to any submission.")]
    Information = 3,

    [GraphQLDescription("Used to indicate an error situation, typically on a submission. Contains a service-specific activityErrorCode.")]
    Error = 4,

    [GraphQLDescription("Indicates that the dialog is closed for further changes. This typically happens when the dialog is completed or deleted.")]
    Closed = 5,

    [GraphQLDescription("When the dialog is forwarded (delegated access) by someone with access to others.")]
    Forwarded = 7
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
