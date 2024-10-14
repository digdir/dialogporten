using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.IdempotentNotifications;

public interface INotificationProcessingContext : IAsyncDisposable
{
    Task Ack(CancellationToken cancellationToken = default);
    Task Nack(CancellationToken cancellationToken = default);
    Task AckHandler(string handlerName, CancellationToken cancellationToken = default);
    Task<bool> HandlerIsAcked(string handlerName, CancellationToken cancellationToken = default);
}

internal sealed class NotificationProcessingContext : INotificationProcessingContext
{
    private readonly SemaphoreSlim _initializeLock = new(1, 1);
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly Action<Guid> _onDispose;
    private readonly Guid _eventId;

    private DialogDbContext? _db;
    private IServiceScope? _serviceScope;
    private bool _acknowledged;

    public NotificationProcessingContext(IServiceScopeFactory serviceScopeFactory, Guid eventId, Action<Guid> onDispose)
    {
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _onDispose = onDispose ?? throw new ArgumentNullException(nameof(onDispose));
        _eventId = eventId;
    }

    public async Task Ack(CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        var existingAckInDatabase = _db.ChangeTracker
            .Entries<NotificationAcknowledgement>()
            .Any(x => x.Entity.EventId == _eventId && x.State
                is EntityState.Unchanged
                or EntityState.Modified);

        if (existingAckInDatabase)
        {
            await _db.NotificationAcknowledgements
                .Where(x => x.EventId == _eventId)
                .ExecuteDeleteAsync(cancellationToken);
        }

        _acknowledged = true;
    }

    public async Task Nack(CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        if (!_acknowledged && _db.ChangeTracker.HasChanges())
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task AckHandler(string handlerName, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _db.NotificationAcknowledgements.AddAsync(new()
        {
            EventId = _eventId,
            NotificationHandler = handlerName
        }, cancellationToken);
    }

    public Task<bool> HandlerIsAcked(string handlerName, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        var acknowledged = _db.NotificationAcknowledgements
            .Local
            .Any(x => x.EventId == _eventId && x.NotificationHandler == handlerName);
        return Task.FromResult(acknowledged);
    }

    public async ValueTask DisposeAsync()
    {
        if (!_acknowledged)
        {
            await Nack();
        }

        _initializeLock.Dispose();
        _serviceScope?.Dispose();
        _serviceScope = null;
        _db = null;
        _onDispose(_eventId);
    }

    internal async Task Initialize(bool isFirstAttempt = false, CancellationToken cancellationToken = default)
    {
        await _initializeLock.WaitAsync(cancellationToken);
        if (_serviceScope is not null)
        {
            return;
        }

        _serviceScope = _serviceScopeFactory.CreateScope();
        _db = _serviceScope.ServiceProvider.GetRequiredService<DialogDbContext>();
        if (!isFirstAttempt)
        {
            await _db.NotificationAcknowledgements
                .Where(x => x.EventId == _eventId)
                .LoadAsync(cancellationToken);
        }
    }

    [MemberNotNull(nameof(_db), nameof(_serviceScope))]
    private void EnsureInitialized()
    {
        if (_db is null || _serviceScope is null)
        {
            throw new InvalidOperationException("Transaction not initialized.");
        }
    }
}
