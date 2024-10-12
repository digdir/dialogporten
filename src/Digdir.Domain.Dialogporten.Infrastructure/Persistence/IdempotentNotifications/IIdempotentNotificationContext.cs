using System.Collections.Concurrent;
using Digdir.Domain.Dialogporten.Domain.Common.EventPublisher;
using Microsoft.Extensions.DependencyInjection;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.IdempotentNotifications;

public interface IIdempotentNotificationContext
{
    Task<IIdempotentNotificationTransaction> BeginTransaction(IDomainEvent domainEvent, bool isFirstAttempt = false, CancellationToken cancellationToken = default);
    IIdempotentNotificationTransaction GetExistingTransaction(Guid eventId);
}

internal sealed class IdempotentNotificationContext : IIdempotentNotificationContext
{
    private readonly ConcurrentDictionary<Guid, IdempotentNotificationTransaction> _transactionsByEventId = new();
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public IdempotentNotificationContext(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }

    public async Task<IIdempotentNotificationTransaction> BeginTransaction(IDomainEvent domainEvent, bool isFirstAttempt = false, CancellationToken cancellationToken = default)
    {
        var transaction = _transactionsByEventId.GetOrAdd(
            key: domainEvent.EventId,
            valueFactory: eventId => new(_serviceScopeFactory, eventId, onDispose: RemoveTransaction));
        await transaction.Initialize(isFirstAttempt, cancellationToken);
        return transaction;
    }

    public IIdempotentNotificationTransaction GetExistingTransaction(Guid eventId)
    {
        return _transactionsByEventId.TryGetValue(eventId, out var transaction)
                ? transaction
                : throw new InvalidOperationException("Transaction not found.");
    }

    private void RemoveTransaction(Guid eventId) => _transactionsByEventId.TryRemove(eventId, out _);
}
