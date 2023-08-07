using Digdir.Domain.Dialogporten.Domain.Http;
using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;

public class DialogApiActionEndpoint : IEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public string? Version { get; set; }
    public Uri Url { get; set; } = null!;
    public Uri? DocumentationUrl { get; set; }
    public Uri? RequestSchema { get; set;}
    public Uri? ResponseSchema { get; set;}
    public bool Deprecated { get; set; }
    public DateTimeOffset? SunsetAt { get; set; }

    // === Dependent relationships ===
    public HttpVerb.Enum HttpMethodId { get; set; }
    public HttpVerb HttpMethod { get; set; } = null!;

    public Guid ActionId { get; set; }
    public DialogApiAction Action { get; set; } = null!;
}
