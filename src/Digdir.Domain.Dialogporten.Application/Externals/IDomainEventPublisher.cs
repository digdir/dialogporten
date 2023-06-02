using Digdir.Domain.Dialogporten.Domain.Common;

namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface IDomainEventPublisher
{
    void Publish(IDomainEvent domainEvent);
    void Publish(IEnumerable<IDomainEvent> domainEvents);
}
