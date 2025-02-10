using Digdir.Library.Entity.Abstractions;
using Digdir.Library.Entity.Abstractions.Features.Immutable;

namespace Digdir.Domain.Dialogporten.Domain.Actors;

public sealed class ActorName : IImmutableEntity
{
    public Guid Id { get; set; }
    public string ActorId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}
