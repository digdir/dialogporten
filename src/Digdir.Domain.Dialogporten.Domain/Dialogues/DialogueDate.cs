using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.Dialogues;

public class DialogueDate : IEntity
{
    public long InternalId { get; set; }
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public Guid UpdatedByUserId { get; set; }

    /// <summary>
    /// Hvis oppgitt blir dialogen satt med en frist 
    /// (i Altinn2 er denne bare retningsgivende og har ingen effekt, skal dette fortsette?)
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Mulighet for å skjule/deaktivere en dialog på et eller annet tidspunkt?
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    public DateTime? ReadDate { get; set; }

    // === Dependent relationships ===
    public long DialogueId { get; set; }
    public DialogueEntity Dialogue { get; set; } = null!;
}
