using Digdir.Domain.Dialogporten.Domain.Outboxes;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence;
using Digdir.Library.Entity.Abstractions.Features.EventPublisher;
using MediatR;

namespace Digdir.Domain.Dialogporten.Infrastructure.DomainEvents.Outbox.Dispatcher;

internal sealed class IdempotentDomainEventHandler<TDomainEvent> : INotificationHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    private readonly INotificationHandler<TDomainEvent> _decorated;
    private readonly DialogDbContext _db;

    public IdempotentDomainEventHandler(
        INotificationHandler<TDomainEvent> decorated,
        DialogDbContext db)
    {
        _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task Handle(TDomainEvent notification, CancellationToken cancellationToken)
    {
        var consumer = _decorated.GetType().Name;
        var outboxMessageConsumer = await _db
            .OutboxMessageConsumers
            .FindAsync([notification.EventId, consumer], cancellationToken);
        if (outboxMessageConsumer is not null)
        {
            // I've handled this event before, so I'm not going to do it again.
            return;
        }

        await _decorated.Handle(notification, cancellationToken);

        _db.OutboxMessageConsumers.Add(new OutboxMessageConsumer
        {
            EventId = notification.EventId,
            ConsumerName = consumer
        });

        await _db.SaveChangesAsync(cancellationToken);
    }
}
