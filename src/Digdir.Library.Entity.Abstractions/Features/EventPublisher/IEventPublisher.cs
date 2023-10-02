namespace Digdir.Library.Entity.Abstractions.Features.EventPublisher;

/// <summary>
/// Abstraction representing functionality to publish domain events.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// A collection of <see cref="IDomainEvent"/> queued up for dispatching at the end of the next unit of work.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
}