using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Domain.Outboxes;
using Digdir.Library.Entity.Abstractions.Features.EventPublisher;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Digdir.Domain.Dialogporten.Infrastructure.DomainEvents.Outbox;

internal sealed class ConvertDomainEventsToOutboxMessagesInterceptor : SaveChangesInterceptor
{
    private readonly ITransactionTime _transactionTime;

    public ConvertDomainEventsToOutboxMessagesInterceptor(ITransactionTime transactionTime)
    {
        _transactionTime = transactionTime ?? throw new ArgumentNullException(nameof(transactionTime));
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

        var domainEvents = dbContext.ChangeTracker.Entries()
            .SelectMany(x =>
                x.Entity is IEventPublisher publisher
                    ? publisher.PopDomainEvents()
                    : Enumerable.Empty<IDomainEvent>())
            .ToList();

        foreach (var domainEvent in domainEvents)
        {
            domainEvent.OccuredAt = _transactionTime.Value;
        }

        var outboxMessages = domainEvents
            .Select(OutboxMessage.Create)
            .ToList();

        dbContext.Set<OutboxMessage>().AddRange(outboxMessages);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
