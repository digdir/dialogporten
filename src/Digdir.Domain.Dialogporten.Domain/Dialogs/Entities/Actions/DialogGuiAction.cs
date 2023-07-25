using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Library.Entity.Abstractions;
using Digdir.Library.Entity.Abstractions.Features;
using Digdir.Library.Entity.Abstractions.Features.Hierarchy;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;

public class DialogGuiAction : IEntity, ISubEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public string Action { get; set; } = null!;
    public Uri Url { get; set; } = null!;
    public string? AuthorizationAttribute { get; set; }
    public bool IsBackChannel { get; set; }
    public bool IsDeleteAction { get; set; }

    public IEntityBase Parent => Dialog;

    // === Dependent relationships ===
    public DialogGuiActionPriority.Enum PriorityId { get; set; }
    public DialogGuiActionPriority Priority { get; set; } = null!;

    public DialogGuiActionTitle? Title { get; set; }

    public Guid DialogId { get; set; }
    public DialogEntity Dialog { get; set; } = null!;
}

public class DialogGuiActionTitle : LocalizationSet
{
    public Guid GuiActionId { get; set; }
    public DialogGuiAction GuiAction { get; set; } = null!;
}