using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.Dialogues.Configuration;

public class DialogueConfiguration : IEntity
{
    public long InternalId { get; set; }
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public Guid UpdatedByUserId { get; set; }

    /// <summary>
    /// Når blir dialogen synlig hos party. Muliggjør opprettelse i forveien og samtidig tilgjengeliggjøring 
    /// for mange parties.
    /// </summary>
    public DateTime VisibleFrom { get; set; }

    // === Dependent relationships ===
    public long DialogueId { get; set; }
    public DialogueEntity Dialogue { get; set; } = null!;

    // == Plural principal relationships ===
    public List<DialogueConfigurationTokenScope> TokenScopes { get; set; } = new();
}
