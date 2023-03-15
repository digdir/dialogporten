using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.Attachments;

public class DialogueAttachement : IEntity
{
    public long InternalId { get; set; }
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public Guid UpdatedByUserId { get; set; }

    public LocalizationSet DisplayName { get; set; } = null!;
    public long SizeInBytes { get; set; }
    public string ContentType { get; set; } = null!;
    public Uri Url { get; set; } = null!;

    /// <summary>
    /// Det kan oppgis en valgfri referanse til en ressurs. Brukeren må ha tilgang til "open" i
    /// XACML-policy for oppgitt ressurs for å få tilgang til dialogen.
    /// </summary>
    public string? Resource { get; set; }

    // === Dependent relationships ===
    public long DialogueId { get; set; }
    public DialogueEntity Dialogue { get; set; } = null!;
}
