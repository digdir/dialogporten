using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Elements;
using Digdir.Library.Entity.Abstractions;
using Digdir.Library.Entity.Abstractions.Features.Aggregate;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;

public class DialogApiAction : IEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public string Action { get; set; } = null!;
    public string? AuthorizationAttribute { get; set; }

    // === Dependent relationships ===
    public Guid DialogId { get; set; }
    public DialogEntity Dialog { get; set; } = null!;

    public Guid? DialogElementId { get; set; }
    public DialogElement? DialogElement { get; set; }

    // === Principal relationships ===
    [AggregateChild]
    public List<DialogApiActionEndpoint> Endpoints { get; set; } = [];
}
