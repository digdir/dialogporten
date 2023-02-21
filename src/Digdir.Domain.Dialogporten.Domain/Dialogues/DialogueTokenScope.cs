using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.Dialogues;

public class DialogueTokenScope : IImutableEntity
{
    public long InternalId { get; set; }
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string Value { get; set; } = null!;

    // === Dependent relationships ===
    public long DialogueId { get; set; }
    public DialogueEntity Dialogue { get; set; } = null!;
}