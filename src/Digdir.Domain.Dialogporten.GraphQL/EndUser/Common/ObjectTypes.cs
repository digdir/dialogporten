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
    public DateTimeOffset SeenAt { get; set; }

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
    [GraphQLDescription("New")]
    New = 1,

    [GraphQLDescription("In progress. General status used for dialog services where further user input is expected.")]
    InProgress = 2,

    [GraphQLDescription("Waiting for feedback from the service provider")]
    Waiting = 3,

    [GraphQLDescription("The dialog is in a state where it is waiting for signing. Typically the last step after all completion is carried out and validated.")]
    Signing = 4,

    [GraphQLDescription("The dialog was cancelled. This typically removes the dialog from normal GUI views.")]
    Cancelled = 5,

    [GraphQLDescription("The dialog was completed. This typically moves the dialog to a GUI archive or similar.")]
    Completed = 6
}
