using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities;
using Digdir.Library.Entity.Abstractions.Features.Immutable;

namespace Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.TokenScopes;

public class DialogueTokenScope : IImmutableEntity
{
    public long InternalId { get; set; }
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public string Value { get; set; } = null!;

    // === Dependent relationships ===
    public long DialogueId { get; set; }
    public DialogueEntity Dialogue { get; set; } = null!;
}
