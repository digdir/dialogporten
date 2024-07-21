using Digdir.Library.Entity.Abstractions.Features.Identifiable;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actors;

public class DialogActor : IIdentifiableEntity
{
    public Guid Id { get; set; }
    public string? ActorId { get; set; }
    public DialogActorType ActorType { get; set; } = null!;
    public DialogActorType.Values ActorTypeId { get; set; }
    public string? ActorName { get; set; } = null!;
}
