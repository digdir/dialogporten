using Digdir.Library.Entity.Abstractions.Features.Immutable;

// ReSharper disable ClassNeverInstantiated.Global

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actors;

public abstract class DialogActor : IImmutableEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? ActorId { get; set; }
    public string? ActorName { get; set; }

    public DialogActorType.Values ActorTypeId { get; set; }
    public DialogActorType ActorType { get; set; } = null!;
}
