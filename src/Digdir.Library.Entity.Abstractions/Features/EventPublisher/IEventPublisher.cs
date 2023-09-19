namespace Digdir.Library.Entity.Abstractions.Features.EventPublisher;

public class IEventPublisher
{
    public IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
}