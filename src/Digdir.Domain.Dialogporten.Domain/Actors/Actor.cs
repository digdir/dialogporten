using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.Actors;

// **********************************************************************************************************************
// Created derived types for PerformedBy, SeenLog, Sender(transmission)
// **********************************************************************************************************************

public class Actor : IEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string? ActorName { get; set; }
    public string? ActorId { get; set; }

    public ActorType.Values ActorTypeId { get; set; }
    public ActorType ActorType { get; set; } = null!;
}
