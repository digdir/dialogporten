using MediatR;

namespace Digdir.Domain.Dialogporten.Domain.Common.EventPublisher;

/// <summary>
/// Represents a domain event.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// The events identification. This can be used by consumers to ensure exactly once processing.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// The time in which the event, as well as all the actual changes occured.
    /// </summary>
    DateTimeOffset OccuredAt { get; set; }
}
