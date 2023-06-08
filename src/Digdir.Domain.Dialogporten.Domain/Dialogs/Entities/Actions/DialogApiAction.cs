using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;

public class DialogApiAction : IEntity
{
    public long InternalId { get; set; }
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }

    public string Action { get; set; } = null!;
    public string? AuthorizationAttribute { get; set; }
    public Uri Url { get; set; } = null!;
    // TODO: Skal vi ha noe strengere validering her?
    public string HttpMethod { get; set; } = null!;
    public Uri? DocumentationUrl { get; set; }
    public Uri? RequestSchema { get; set; }
    public Uri? ResponseSchema { get; set; }

    // === Dependent relationships ===
    public long DialogId { get; set; }
    public DialogEntity Dialog { get; set; } = null!;
}
