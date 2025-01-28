using MediatR;

namespace Digdir.Domain.Dialogporten.Domain.Common.EventPublisher;

/// <summary>
/// Represents a domain event.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// The events' identification. Consumers can use this to ensure exactly once processing.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// The time at which the event, as well as all the actual changes, occurred.
    /// </summary>
    // This contains a typo and will be removed as soon as we are
    // sure there are no references to it remaining on the message bus.
    DateTimeOffset OccuredAt { get; set; }

    /// <summary>
    /// The time at which the event, as well as all the actual changes, occurred.
    /// </summary>
    DateTimeOffset OccurredAt
    {
        get => OccuredAt;
        set => OccuredAt = value;
    }

    /// <summary>
    /// Dictionary of metadata.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; }
}
