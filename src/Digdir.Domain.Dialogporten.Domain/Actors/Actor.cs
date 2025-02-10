using Digdir.Library.Entity.Abstractions;

// ReSharper disable ClassNeverInstantiated.Global

namespace Digdir.Domain.Dialogporten.Domain.Actors;

public abstract class Actor : IEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string? ActorId { get; set; }
    public string? ActorName { get; set; }

    public ActorType.Values ActorTypeId { get; set; }
    public ActorType ActorType { get; set; } = null!;

    public ActorName? ActorNameEntity { get; set; }
}
