using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.Dialogues;

public class DialogueConfiguration : IEntity
{
    public long InternalId { get; set; }
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public Guid UpdatedByUserId { get; set; }

    ///// <summary>
    ///// Tjenestetilbyder kan oppgi et selvpålagt tokenkrav, som innebærer at dette dialogen vil kreve at det 
    ///// autoriseres med et Maskinporten-token som inneholder følgende scopes i tillegg til 
    ///// </summary>
    //public List<string> ServiceProviderScopesRequired { get; set; } = new();

    /// <summary>
    /// Når blir dialogen synlig hos party. Muliggjør opprettelse i forveien og samtidig tilgjengeliggjøring 
    /// for mange parties.
    /// </summary>
    public DateTime VisibleFrom { get; set; }

    // === Dependent relationships ===
    public long DialogueId { get; set; }
    public DialogueEntity Dialogue { get; set; } = null!;
}