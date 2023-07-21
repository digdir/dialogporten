using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;
using Digdir.Library.Entity.Abstractions;
using Digdir.Library.Entity.Abstractions.Features;
using Digdir.Library.Entity.Abstractions.Features.Hierarchy;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;

public class DialogApiAction : IEntity, ISubEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public string Action { get; set; } = null!;
    public string? AuthorizationAttribute { get; set; }

    public IEntityBase Parent => Dialog;

    // === Dependent relationships ===
    public Guid DialogId { get; set; }
    public DialogEntity Dialog { get; set; } = null!;

    public Guid? DialogElementId { get; set; }
    public DialogElement? DialogElement { get; set; }

    // === Principal relationships ===
    public List<DialogApiActionEndpoint> Endpoints { get; set; } = new();
}
