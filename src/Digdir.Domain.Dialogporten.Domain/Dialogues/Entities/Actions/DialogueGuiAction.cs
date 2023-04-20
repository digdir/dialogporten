using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.Actions;

public class DialogueGuiAction : IEntity
{
    public long InternalId { get; set; }
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public Guid UpdatedByUserId { get; set; }

    public string Action { get; set; } = null!;
    public LocalizationSet Title { get; set; } = null!;
    public Uri Url { get; set; } = null!;
    public string? Resource { get; set; }
    public bool IsBackChannel { get; set; }
    public bool IsDeleteAction { get; set; }

    // === Dependent relationships ===
    public DialogueGuiActionType.Enum TypeId { get; set; }
    public DialogueGuiActionType Type { get; set; } = null!;

    public long DialogueId { get; set; }
    public DialogueEntity Dialogue { get; set; } = null!;
}
