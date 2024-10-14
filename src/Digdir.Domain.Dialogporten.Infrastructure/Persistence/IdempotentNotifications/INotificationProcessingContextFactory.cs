using System.Collections.Concurrent;
using Digdir.Domain.Dialogporten.Domain.Common.EventPublisher;
using Microsoft.Extensions.DependencyInjection;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.IdempotentNotifications;

public interface INotificationProcessingContextFactory
{
    Task<INotificationProcessingContext> CreateContext(IDomainEvent domainEvent, bool isFirstAttempt = false, CancellationToken cancellationToken = default);
    INotificationProcessingContext GetExistingContext(Guid eventId);
}

internal sealed class NotificationProcessingContextFactory : INotificationProcessingContextFactory
{
    private readonly ConcurrentDictionary<Guid, NotificationProcessingContext> _contextByEventId = new();
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public NotificationProcessingContextFactory(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }

    public async Task<INotificationProcessingContext> CreateContext(
        IDomainEvent domainEvent,
        bool isFirstAttempt = false,
        CancellationToken cancellationToken = default)
    {
        var transaction = _contextByEventId.GetOrAdd(
            key: domainEvent.EventId,
            valueFactory: eventId => new(_serviceScopeFactory, eventId, onDispose: RemoveTransaction));
        await transaction.Initialize(isFirstAttempt, cancellationToken);
        return transaction;
    }

    public INotificationProcessingContext GetExistingContext(Guid eventId)
    {
        return _contextByEventId.TryGetValue(eventId, out var transaction)
                ? transaction
                : throw new InvalidOperationException("Notification context not found.");
    }

    private void RemoveTransaction(Guid eventId) => _contextByEventId.TryRemove(eventId, out _);
}
