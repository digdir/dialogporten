using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;
using Digdir.Library.Entity.Abstractions.Features.Identifiable;
// ReSharper disable ClassNeverInstantiated.Global

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actors;

public sealed class DialogActivityPerformedByActor : Actor
{
    public Guid ActivityId { get; set; }
    public DialogActivity Activity { get; set; } = null!;
}

public sealed class DialogSeenLogSeenByActor : Actor
{
    public Guid DialogSeenLogId { get; set; }
    public DialogSeenLog DialogSeenLog { get; set; } = null!;
}

public sealed class DialogTransmissionSenderActor : Actor
{
    public Guid TransmissionId { get; set; }
    public DialogTransmission Transmission { get; set; } = null!;
}

public abstract class Actor : IIdentifiableEntity
{
    public Guid Id { get; set; }
    public string? ActorId { get; set; }
    public DialogActorType ActorType { get; set; } = null!;
    public DialogActorType.Values ActorTypeId { get; set; }
    public string? ActorName { get; set; } = null!;
}
