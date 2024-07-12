using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

// **********************************************************************************************************************
// Rename to Actor
// Move to same folder level as Dialog/Localization
// Created derived types for PerformedBy, SeenLog, Sender(transmission) (and maybe dialog.party / dialog.org)
// **********************************************************************************************************************

public class DialogActor : IEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string? ActorName { get; set; }
    public string? ActorId { get; set; }

    // === Dependent relationships ===
    // public Guid? DialogId { get; set; }
    // public DialogEntity Dialog { get; set; } = null!;
    //
    // public Guid? ActivityId { get; set; }
    // public DialogActivity Activity { get; set; } = null!;
    //
    // TODO: Add transmissions

    public ActorType.Values ActorTypeId { get; set; }
    public ActorType ActorType { get; set; } = null!;
}
