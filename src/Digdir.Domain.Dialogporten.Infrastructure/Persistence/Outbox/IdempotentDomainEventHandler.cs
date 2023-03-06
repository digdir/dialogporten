using Digdir.Domain.Dialogporten.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Outbox;

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
        string consumer = _decorated.GetType().Name;
        var isHandledByConsumer = await _db.Set<OutboxMessageConsumer>()
            .AnyAsync(x => x.Id == notification.EventId && x.Name == consumer, cancellationToken);
        if (isHandledByConsumer)
        {
            return;
        }

        await _decorated.Handle(notification, cancellationToken);

        _db.Set<OutboxMessageConsumer>()
            .Add(new OutboxMessageConsumer
            {
                Id = notification.EventId,
                Name = consumer
            });
        await _db.SaveChangesAsync(cancellationToken);
    }
}