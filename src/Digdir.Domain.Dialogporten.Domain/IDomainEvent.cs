using MediatR;

namespace Digdir.Domain.Dialogporten.Domain;

public interface IDomainEvent : INotification
{
    Guid EventId { get; }
}

public abstract record DomainEvent(Guid EventId) : IDomainEvent;

public sealed record DialogueCreatedDomainEvent(Guid EventId, Guid DialogueId) : DomainEvent(EventId);

public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => _domainEvents.ToList().AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => 
        _domainEvents.Add(domainEvent);

}

// TODO: Raise domain event fra domenelaget eller app laget? 