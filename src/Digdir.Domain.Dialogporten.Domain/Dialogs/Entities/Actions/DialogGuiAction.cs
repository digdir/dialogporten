using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;

public class DialogGuiAction : IEntity
{
    public long InternalId { get; set; }
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }

    public string Action { get; set; } = null!;
    public LocalizationSet Title { get; set; } = null!;
    public Uri Url { get; set; } = null!;
    public string? AuthorizationAttribute { get; set; }
    public bool IsBackChannel { get; set; }
    public bool IsDeleteAction { get; set; }

    // === Dependent relationships ===
    public DialogGuiActionType.Enum TypeId { get; set; }
    public DialogGuiActionType Type { get; set; } = null!;

    public long DialogId { get; set; }
    public DialogEntity Dialog { get; set; } = null!;
}
