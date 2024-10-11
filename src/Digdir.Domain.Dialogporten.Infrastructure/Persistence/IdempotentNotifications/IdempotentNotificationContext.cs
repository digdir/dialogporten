using Digdir.Domain.Dialogporten.Application.Common.Behaviours;
using Digdir.Domain.Dialogporten.Domain.Common.EventPublisher;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.IdempotentNotifications;

internal sealed class IdempotentNotificationContext : IIdempotentNotificationContext
{
    private readonly ILogger<IdempotentNotificationContext> _logger;
    private readonly DialogDbContext _db;

    public IdempotentNotificationContext(DialogDbContext db,
        ILogger<IdempotentNotificationContext> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public Task AcknowledgePart(NotificationAcknowledgementPart acknowledgementPart,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Acknowledging notification with event id {EventId} for {NotificationHandler}",
            acknowledgementPart.EventId,
            acknowledgementPart.NotificationHandler);
        _db.NotificationAcknowledgements.Add(new()
        {
            EventId = acknowledgementPart.EventId,
            NotificationHandler = acknowledgementPart.NotificationHandler
        });
        return Task.CompletedTask;
    }

    public Task<bool> IsAcknowledged(NotificationAcknowledgementPart acknowledgementPart, CancellationToken cancellationToken = default)
    {
        var acknowledged = _db.NotificationAcknowledgements.Local
            .Any(x => x.EventId == acknowledgementPart.EventId
                && x.NotificationHandler == acknowledgementPart.NotificationHandler);
        return Task.FromResult(acknowledged);
    }

    public async Task AcknowledgeWhole(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var deletedCount = await _db.NotificationAcknowledgements
            .Where(x => x.EventId == domainEvent.EventId)
            .ExecuteDeleteAsync(cancellationToken);
        _logger.LogDebug(
            "Notification acknowledged with event id {EventId} - {DeletedCount} acknowledgements removed.",
            domainEvent.EventId,
            deletedCount);
    }

    public Task NotAcknowledgeWhole(CancellationToken cancellationToken = default)
        => _db.ChangeTracker.HasChanges()
            ? _db.SaveChangesAsync(cancellationToken)
            : Task.CompletedTask;

    public async Task LoadAcknowledgements(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Loading notification with event id {EventId}", domainEvent.EventId);
        await _db.NotificationAcknowledgements
            .Where(x => x.EventId == domainEvent.EventId)
            .LoadAsync(cancellationToken);
    }
}
