using Digdir.Domain.Dialogporten.Domain.Common;
using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.Dialogues;

public class DialogueContent : IEntity
{
    public long InternalId { get; set; }
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public Guid UpdatedByUserId { get; set; }

    // === Dependent relationships ===
    public long DialogueId { get; set; }
    public DialogueEntity Dialogue { get; set; } = null!;

    // === Single principal relationships ===
    public LocalizationSet Body { get; set; } = null!;
    public LocalizationSet Title { get; set; } = null!;
    public LocalizationSet SenderName { get; set; } = null!;
    public LocalizationSet SearchTitle { get; set; } = null!;
}
