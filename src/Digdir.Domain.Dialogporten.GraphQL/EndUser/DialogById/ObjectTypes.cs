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
