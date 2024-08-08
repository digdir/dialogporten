using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Transmissions;
using Digdir.Library.Entity.Abstractions.Features.Immutable;

// ReSharper disable ClassNeverInstantiated.Global

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actors;

public sealed class DialogActivityPerformedByActor : DialogActor
{
    public Guid ActivityId { get; set; }
    public DialogActivity Activity { get; set; } = null!;
}

public sealed class DialogSeenLogSeenByActor : DialogActor
{
    public Guid DialogSeenLogId { get; set; }
    public DialogSeenLog DialogSeenLog { get; set; } = null!;
}

public sealed class DialogTransmissionSenderActor : DialogActor
{
    public Guid TransmissionId { get; set; }
    public DialogTransmission Transmission { get; set; } = null!;
}

public abstract class DialogActor : IImmutableEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? ActorId { get; set; }
    public string? ActorName { get; set; }

    public DialogActorType.Values ActorTypeId { get; set; }
    public DialogActorType ActorType { get; set; } = null!;
}
