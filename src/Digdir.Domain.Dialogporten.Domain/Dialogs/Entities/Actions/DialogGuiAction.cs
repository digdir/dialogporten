using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Library.Entity.Abstractions;
using Digdir.Library.Entity.Abstractions.Features.Aggregate;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;

public class DialogGuiAction : IEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public string Action { get; set; } = null!;
    public Uri Url { get; set; } = null!;
    public string? AuthorizationAttribute { get; set; }
    public bool IsBackChannel { get; set; }
    public bool IsDeleteAction { get; set; }

    // === Dependent relationships ===
    public DialogGuiActionPriority.Enum PriorityId { get; set; }
    public DialogGuiActionPriority Priority { get; set; } = null!;

    public DialogGuiActionTitle? Title { get; set; }

    [AggregateParent]
    public DialogEntity Dialog { get; set; } = null!;
    public Guid DialogId { get; set; }
}

public class DialogGuiActionTitle : LocalizationSet
{
    [AggregateParent]
    public DialogGuiAction GuiAction { get; set; } = null!;
    public Guid GuiActionId { get; set; }
}