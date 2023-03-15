using Digdir.Domain.Dialogporten.Infrastructure.DomainEvents.Outbox.Entities;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Digdir.Domain.Dialogporten.Infrastructure.DomainEvents.Outbox;

internal sealed class ConvertDomainEventsToOutboxMessagesInterceptor : SaveChangesInterceptor
{
    private readonly DomainEventPublisher _eventPublisher;

    public ConvertDomainEventsToOutboxMessagesInterceptor(DomainEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;

        if (dbContext is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var outboxMessages = _eventPublisher
            .GetDomainEvents()
            .Select(domainEvent => OutboxMessage.Create(domainEvent, DateTime.UtcNow))
            .ToList();
        _eventPublisher.ClearDomainEvents();

        dbContext.Set<OutboxMessage>().AddRange(outboxMessages);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
