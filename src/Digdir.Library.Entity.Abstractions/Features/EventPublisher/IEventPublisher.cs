namespace Digdir.Library.Entity.Abstractions.Features.EventPublisher;

public interface IEventPublisher
{
    public IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
}