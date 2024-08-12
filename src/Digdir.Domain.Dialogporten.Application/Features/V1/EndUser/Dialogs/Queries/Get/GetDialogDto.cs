using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Content;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actors;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Attachments;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;
using Digdir.Domain.Dialogporten.Domain.Http;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Get;

public sealed class GetDialogDto
{
    /// <summary>
    /// The unique identifier for the in UUIDv7 format.
    /// </summary>
    /// <example>01913cd5-784f-7d3b-abef-4c77b1f0972d</example>
    public Guid Id { get; set; }

    /// <summary>
    /// The unique identifier for the revision in UUIDv4 format.
    /// </summary>
    /// <example>a312cb9c-7632-43c2-aa38-69b06aed56ca</example>
    public Guid Revision { get; set; }

    /// <summary>
    /// The service owner code representing the organization (service owner) related to this dialog.
    /// </summary>
    /// <example>ske</example>
    public string Org { get; set; } = null!;

    /// <summary>
    /// The service identifier for the service that the dialog is related to in URN-format.
    /// This corresponds to a service resource in the Altinn Resource Registry.
    /// </summary>
    /// <example>urn:altinn:resource:some-service-identifier</example>
    public string ServiceResource { get; set; } = null!;

    /// <summary>
    /// The party code representing the organization or person that the dialog belongs to in URN format
    /// </summary>
    /// <example>
    /// urn:altinn:person:identifier-no:01125512345
    /// urn:altinn:organization:identifier-no:912345678
    /// </example>
    public string Party { get; set; } = null!;

    /// <summary>
    /// Advisory indicator of progress, represented as 1-100 percentage value. 100% representing a dialog that has come
    /// to a natural completion (successful or not).
    /// </summary>
    public int? Progress { get; set; }

    /// <summary>
    /// Arbitrary string with a service-specific indicator of status, typically used to indicate a fine-grained state of
    /// the dialog to further specify the "status" enum.
    ///
    /// Refer to the service-specific documentation provided by the service owner for details on the possible values (if
    /// in use).
    /// </summary>
    public string? ExtendedStatus { get; set; }

    /// <summary>
    /// Arbitrary string with a service-specific reference to an external system or service.
    ///
    /// Refer to the service-specific documentation provided by the service owner for details (if in use).
    /// </summary>
    public string? ExternalReference { get; set; }

    /// <summary>
    /// The due date for the dialog. This is the last date when the dialog is expected to be completed.
    /// </summary>
    /// <example>2022-12-31T23:59:59Z</example>
    public DateTimeOffset? DueAt { get; set; }

    /// <summary>
    /// The expiration date for the dialog. This is the last date when the dialog is available for the end user.
    ///
    /// After this date is passed, the dialog will be considered expired and no longer available for the end user in any
    /// API. If not supplied, the dialog will be considered to never expire. This field can be changed by the service
    /// owner after the dialog has been created.
    /// </summary>
    /// <example>2022-12-31T23:59:59Z</example>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    /// The date and time when the dialog was created.
    /// </summary>
    /// <example>2022-12-31T23:59:59Z</example>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// The date and time when the dialog was last updated.
    /// </summary>
    /// <example>2022-12-31T23:59:59Z</example>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// The aggregated status of the dialog.
    /// </summary>
    public DialogStatus.Values Status { get; set; }

    /// <summary>
    /// The dialog unstructured text content
    /// </summary>
    public GetDialogContentDto Content { get; set; } = null!;

    /// <summary>
    /// The dialog token. May be used (if supported) against external URLs referred to in this dialog's apiActions,
    /// transmissions or attachments. Should also be used for front-channel embeds.
    /// </summary>
    public string? DialogToken { get; set; }

    /// <summary>
    /// The attachments associated with the dialog (on an aggregate level)
    /// </summary>
    public List<GetDialogDialogAttachmentDto> Attachments { get; set; } = [];

    /// <summary>
    /// The immutable list of transmissions associated with the dialog
    /// </summary>
    public List<GetDialogDialogTransmissionDto> Transmissions { get; set; } = [];

    /// <summary>
    /// The GUI actions associated with the dialog. Should be used in browser based interactive front-ends.
    /// </summary>
    public List<GetDialogDialogGuiActionDto> GuiActions { get; set; } = [];

    /// <summary>
    /// The API actions associated with the dialog. Should be used in specialized, non-browser based integrations.
    /// </summary>
    public List<GetDialogDialogApiActionDto> ApiActions { get; set; } = [];

    /// <summary>
    /// An immutable list of activities associated with the dialog.
    /// </summary>
    public List<GetDialogDialogActivityDto> Activities { get; set; } = [];

    /// <summary>
    /// The list of seen log entries for the dialog newer than the dialog ChangedAt date.
    /// </summary>
    public List<GetDialogDialogSeenLogDto> SeenSinceLastUpdate { get; set; } = [];
}

public sealed class GetDialogDialogTransmissionDto
{
    /// <summary>
    /// The unique identifier for the transmission in UUIDv7 format.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The date and time when the transmission was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Authorization attribute specifying the required authorization in order to access the embedded content or
    /// attachments for the transmission.
    /// </summary>
    public string? AuthorizationAttribute { get; set; }

    /// <summary>
    /// Flag indicating if the authenticated user is authorized for this transmission. If not, embedded content and
    /// the attachments will not be available
    /// </summary>
    public bool IsAuthorized { get; set; }

    /// <summary>
    /// Arbitrary string with a service-specific transmission type.
    ///
    /// Refer to the service-specific documentation provided by the service owner for details (if in use).
    /// </summary>
    public string? ExtendedType { get; set; }

    /// <summary>
    /// Reference to any other transmission that this transmission is related to.
    /// </summary>
    public Guid? RelatedTransmissionId { get; set; }

    /// <summary>
    /// The type of transmission.
    /// </summary>
    public DialogTransmissionType.Values Type { get; set; }

    /// <summary>
    /// The actor that sent the transmission.
    /// </summary>
    public GetDialogDialogTransmissionSenderActorDto Sender { get; set; } = null!;

    /// <summary>
    /// The transmission unstructured text content
    /// </summary>
    public GetDialogDialogTransmissionContentDto Content { get; set; } = null!;

    /// <summary>
    /// The transmission-level attachments
    /// </summary>
    public List<GetDialogTransmissionAttachmentDto> Attachments { get; set; } = [];
}

public sealed class GetDialogDialogSeenLogDto
{
    /// <summary>
    /// The unique identifier for the seen log entry in UUIDv7 format.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The timestamp when the dialog revision was seen
    /// </summary>
    public DateTimeOffset SeenAt { get; set; }

    /// <summary>
    /// The actor that saw the dialog revision
    /// </summary>
    public GetDialogDialogSeenLogSeenByActorDto SeenBy { get; set; } = null!;

    /// <summary>
    /// Flag indicating whether the seen log entry was created via the service owner.
    ///
    /// This is used when the service owner uses the service owner API to implement its own frontend.
    /// </summary>
    public bool? IsViaServiceOwner { get; set; }

    /// <summary>
    /// Flag indicating whether the seen log entry was created by the current end user.
    /// </summary>
    public bool IsCurrentEndUser { get; set; }
}

public sealed class GetDialogDialogSeenLogSeenByActorDto
{
    /// <summary>
    /// The natural name of the person/business that saw the dialog revision.
    /// </summary>
    public string ActorName { get; set; } = null!;

    /// <summary>
    /// The identifier of the person/business that saw the dialog revision.
    /// </summary>
    /// <example>
    /// urn:altinn:person:identifier-no:01125512345
    /// urn:altinn:organization:identifier-no:912345678
    /// </example>
    public string ActorId { get; set; } = null!;
}

public sealed class GetDialogDialogTransmissionSenderActorDto
{
    public Guid Id { get; set; }
    public DialogActorType.Values ActorType { get; set; }
    public string ActorName { get; set; } = null!;
    public string ActorId { get; set; } = null!;
}

public sealed class GetDialogContentDto
{
    /// <summary>
    /// The title of the dialog. Always text/plain.
    /// </summary>
    public ContentValueDto Title { get; set; } = null!;

    /// <summary>
    /// A short summary of the dialog and its current state. Always text/plain.
    /// </summary>
    public ContentValueDto Summary { get; set; } = null!;

    /// <summary>
    /// Overridden sender name. If not supplied, assume "org" as the sender name. Always text/plain.
    /// </summary>
    public ContentValueDto? SenderName { get; set; }

    /// <summary>
    /// Additional information about the dialog, this may contain Markdown.
    /// </summary>
    public ContentValueDto? AdditionalInfo { get; set; }

    /// <summary>
    /// Used as the human-readable label used to describe the "ExtendedStatus" field. Always text/plain.
    /// </summary>
    public ContentValueDto? ExtendedStatus { get; set; }

    /// <summary>
    /// Front-channel embedded content. Used to dynamically embed content in the front-end from an external URL.
    /// </summary>
    public ContentValueDto? MainContentReference { get; set; }
}

public sealed class GetDialogDialogTransmissionContentDto
{
    /// <summary>
    /// The transmission title. Always text/plain.
    /// </summary>
    public ContentValueDto Title { get; set; } = null!;

    /// <summary>
    /// The transmission summary. Always text/plain.
    /// </summary>
    public ContentValueDto Summary { get; set; } = null!;
}

public sealed class GetDialogDialogActivityDto
{
    public Guid Id { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public Uri? ExtendedType { get; set; }

    public DialogActivityType.Values Type { get; set; }

    public Guid? RelatedActivityId { get; set; }
    public Guid? TransmissionId { get; set; }

    public GetDialogDialogActivityPerformedByActorDto PerformedBy { get; set; } = null!;
    public List<LocalizationDto> Description { get; set; } = [];
}

public sealed class GetDialogDialogActivityPerformedByActorDto
{
    public Guid Id { get; set; }
    public DialogActorType.Values ActorType { get; set; }
    public string? ActorName { get; set; }
    public string? ActorId { get; set; }
}

public sealed class GetDialogDialogApiActionDto
{
    public Guid Id { get; set; }
    public string Action { get; set; } = null!;
    public string? AuthorizationAttribute { get; set; }
    public bool IsAuthorized { get; set; }

    public List<GetDialogDialogApiActionEndpointDto> Endpoints { get; set; } = [];
}

public sealed class GetDialogDialogApiActionEndpointDto
{
    public Guid Id { get; set; }
    public string? Version { get; set; }
    public Uri Url { get; set; } = null!;
    public HttpVerb.Values HttpMethod { get; set; }
    public Uri? DocumentationUrl { get; set; }
    public Uri? RequestSchema { get; set; }
    public Uri? ResponseSchema { get; set; }
    public bool Deprecated { get; set; }
    public DateTimeOffset? SunsetAt { get; set; }
}

public sealed class GetDialogDialogGuiActionDto
{
    public Guid Id { get; set; }
    public string Action { get; set; } = null!;
    public Uri Url { get; set; } = null!;
    public string? AuthorizationAttribute { get; set; }
    public bool IsAuthorized { get; set; }
    public bool IsDeleteDialogAction { get; set; }

    public DialogGuiActionPriority.Values Priority { get; set; }
    public HttpVerb.Values HttpMethod { get; set; }

    public List<LocalizationDto> Title { get; set; } = [];
    public List<LocalizationDto>? Prompt { get; set; } = [];
}

public sealed class GetDialogDialogAttachmentDto
{
    public Guid Id { get; set; }

    public List<LocalizationDto> DisplayName { get; set; } = [];
    public List<GetDialogDialogAttachmentUrlDto> Urls { get; set; } = [];
}

public sealed class GetDialogDialogAttachmentUrlDto
{
    public Guid Id { get; set; }
    public Uri Url { get; set; } = null!;
    public string? MediaType { get; set; } = null!;

    public AttachmentUrlConsumerType.Values ConsumerType { get; set; }
}

public sealed class GetDialogTransmissionAttachmentDto
{
    public Guid Id { get; set; }

    public List<LocalizationDto> DisplayName { get; set; } = [];
    public List<GetDialogTransmissionAttachmentUrlDto> Urls { get; set; } = [];
}

public sealed class GetDialogTransmissionAttachmentUrlDto
{
    public Guid Id { get; set; }
    public Uri Url { get; set; } = null!;
    public string? MediaType { get; set; } = null!;

    public AttachmentUrlConsumerType.Values ConsumerType { get; set; }
}
