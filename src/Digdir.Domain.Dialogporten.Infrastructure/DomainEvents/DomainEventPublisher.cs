using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Common;

namespace Digdir.Domain.Dialogporten.Infrastructure.DomainEvents;

internal sealed class DomainEventPublisher : IDomainEventPublisher
{
    private readonly HashSet<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> PopDomainEvents()
    {
        var domainEvents = _domainEvents.ToList().AsReadOnly();
        _domainEvents.Clear();
        return domainEvents;
    }

    public void Publish(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    public void Publish(IEnumerable<IDomainEvent> domainEvents)
    {
        ArgumentNullException.ThrowIfNull(domainEvents);
        foreach (var domainEvent in domainEvents)
        {
            Publish(domainEvent);
        }
    }
}
