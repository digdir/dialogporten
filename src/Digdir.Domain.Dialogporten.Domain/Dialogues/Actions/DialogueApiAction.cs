using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.Dialogues.Actions;

public class DialogueApiAction : IEntity
{
    public long InternalId { get; set; }
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public Guid UpdatedByUserId { get; set; }

    public string Action { get; set; } = null!;
    public Uri Url { get; set; } = null!;
    // TODO: Skal vi ha noe strengere validering her?
    public string HttpMethod { get; set; } = null!;
    public string? Resource { get; set; }
    public bool IsBackChannel { get; set; }
    public bool IsDeleteAction { get; set; }

    public Uri? DocumentationUrl { get; set; }
    public Uri? RequestSchema { get; set; }
    // TODO: Hvorfor trenger vi dette?
    public Uri? ResponseSchema { get; set; }

    // === Dependent relationships ===
    public long DialogueId { get; set; }
    public DialogueEntity Dialogue { get; set; } = null!;
}
