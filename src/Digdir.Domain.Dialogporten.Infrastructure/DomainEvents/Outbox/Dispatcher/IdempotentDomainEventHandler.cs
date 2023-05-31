using Digdir.Domain.Dialogporten.Domain.Common;
using Digdir.Domain.Dialogporten.Domain.Outboxes;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Infrastructure.DomainEvents.Outbox.Dispatcher;

internal sealed class IdempotentDomainEventHandler<TDomainEvent> : INotificationHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    private readonly INotificationHandler<TDomainEvent> _decorated;
    private readonly DialogueDbContext _db;

    public IdempotentDomainEventHandler(
        INotificationHandler<TDomainEvent> decorated,
        DialogueDbContext db)
    {
        _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task Handle(TDomainEvent notification, CancellationToken cancellationToken)
    {
        var consumer = _decorated.GetType().Name;
        var isHandledByConsumer = await _db.Set<OutboxMessageConsumer>()
            .AnyAsync(x => x.EventId == notification.EventId && x.ConsumerName == consumer, cancellationToken);
        if (isHandledByConsumer)
        {
            return;
        }

        await _decorated.Handle(notification, cancellationToken);

        _db.Set<OutboxMessageConsumer>().Add(new OutboxMessageConsumer
        {
            EventId = notification.EventId,
            ConsumerName = consumer
        });
        await _db.SaveChangesAsync(cancellationToken);
    }
}