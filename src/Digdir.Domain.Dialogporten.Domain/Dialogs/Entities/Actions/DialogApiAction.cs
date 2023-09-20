using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;
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
    [AggregateParent]
    public DialogEntity Dialog { get; set; } = null!;
    public Guid DialogId { get; set; }

    public DialogElement? DialogElement { get; set; }
    public Guid? DialogElementId { get; set; }

    // === Principal relationships ===
    public List<DialogApiActionEndpoint> Endpoints { get; set; } = new();
}
