using Digdir.Domain.Dialogporten.GraphQL.EndUser.Common;

namespace Digdir.Domain.Dialogporten.GraphQL.EndUser.DialogById;

[InterfaceType("DialogByIdError")]
public interface IDialogByIdError
{
    public string Message { get; set; }
}

public sealed class DialogByIdNotFound : IDialogByIdError
{
    public string Message { get; set; } = null!;
}

public sealed class DialogByIdDeleted : IDialogByIdError
{
    public string Message { get; set; } = null!;
}

public sealed class DialogByIdForbidden : IDialogByIdError
{
    public string Message { get; set; } = "Forbidden";
}

public sealed class DialogByIdPayload
{
    public Dialog? Dialog { get; set; }
    public List<IDialogByIdError> Errors { get; set; } = [];
}

public sealed class Dialog
{
    public Guid Id { get; init; }
    public Guid Revision { get; init; }
    public string Org { get; init; } = null!;
    public string ServiceResource { get; init; } = null!;
    public string Party { get; init; } = null!;
    public int? Progress { get; init; }
    public string? ExtendedStatus { get; init; }
    public string? ExternalReference { get; init; }
    public DateTimeOffset? VisibleFrom { get; init; }
    public DateTimeOffset? DueAt { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    public string? DialogToken { get; init; }

    public DialogStatus Status { get; init; }

    public List<Content> Content { get; init; } = [];
    public List<Element> Elements { get; init; } = [];
    public List<GuiAction> GuiActions { get; init; } = [];
    public List<ApiAction> ApiActions { get; init; } = [];
    public List<Activity> Activities { get; init; } = [];
    public List<SeenLog> SeenSinceLastUpdate { get; init; } = [];
}

public sealed class ApiAction
{
    public Guid Id { get; init; }
    public string Action { get; init; } = null!;
    public string? AuthorizationAttribute { get; init; }
    public bool IsAuthorized { get; init; }

    public Guid? DialogElementId { get; init; }

    public List<ApiActionEndpoint> Endpoints { get; init; } = [];
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
    public Guid Id { get; init; }
    public string? Version { get; init; }
    public Uri Url { get; init; } = null!;
    public HttpVerb HttpMethod { get; init; }
    public Uri? DocumentationUrl { get; init; }
    public Uri? RequestSchema { get; init; }
    public Uri? ResponseSchema { get; init; }
    public bool Deprecated { get; init; }
    public DateTimeOffset? SunsetAt { get; init; }
}

public sealed class GuiAction
{
    public Guid Id { get; init; }
    public string Action { get; init; } = null!;
    public Uri Url { get; init; } = null!;
    public string? AuthorizationAttribute { get; init; }
    public bool IsAuthorized { get; init; }
    public bool IsBackChannel { get; init; }
    public bool IsDeleteAction { get; init; }

    public GuiActionPriority Priority { get; init; }

    public List<Localization> Title { get; init; } = [];
}

public enum GuiActionPriority
{
    Primary = 1,
    Secondary = 2,
    Tertiary = 3
}

public sealed class Element
{
    public Guid Id { get; init; }
    public Uri? Type { get; init; }
    public string? ExternalReference { get; init; }
    public string? AuthorizationAttribute { get; init; }
    public bool IsAuthorized { get; init; }

    public Guid? RelatedDialogElementId { get; init; }

    public List<Localization> DisplayName { get; init; } = [];
    public List<ElementUrl> Urls { get; init; } = [];
}

public sealed class ElementUrl
{
    public Guid Id { get; init; }
    public Uri Url { get; init; } = null!;
    public string? MimeType { get; init; }

    public ElementUrlConsumer ConsumerType { get; init; }
}

public enum ElementUrlConsumer
{
    Gui = 1,
    Api = 2
}
