using Digdir.Domain.Dialogporten.Domain.Outboxes;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;

namespace Digdir.Domain.Dialogporten.Infrastructure.DomainEvents.Outbox.Dispatcher;

internal sealed class OutboxDispatcher
{
    private static readonly Assembly EventAssembly = typeof(OutboxMessage).Assembly;
    private static readonly AsyncRetryPolicy RetryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(50 * attempt));

    private readonly DialogDbContext _db;
    private readonly IMediator _mediatr;
    private readonly ILogger<OutboxDispatcher> _logger;

    public OutboxDispatcher(
        DialogDbContext db,
        IMediator mediatr,
        ILogger<OutboxDispatcher> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _mediatr = mediatr ?? throw new ArgumentNullException(nameof(mediatr));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Execute(CancellationToken cancellationToken = default)
    {
        var outboxMessages = await _db.OutboxMessages
            .Include(x => x.OutboxMessageConsumers)
            .Take(100)
            .ToListAsync(cancellationToken);

        foreach (var outboxMessage in outboxMessages)
        {
            if (!TryToDomainEvent(outboxMessage, out var domainEvent))
            {
                _logger.LogError("Unable to parse OutboxMessage {EventType} {EventId}. Will not attempt again.",
                    outboxMessage.EventType,
                    outboxMessage.EventId);
                _db.OutboxMessages.Remove(outboxMessage);
                await _db.SaveChangesAsync(cancellationToken);
                continue;
            }

            var result = await RetryPolicy.ExecuteAndCaptureAsync(
                    ct => _mediatr.Publish(domainEvent, ct),
                    cancellationToken);

            if (result.Outcome == OutcomeType.Failure)
            {
                _logger.LogError(result.FinalException, "Failed to process OutboxMessage {EventType} {EventId}. Will not attempt again.",
                    outboxMessage.EventType,
                    outboxMessage.EventId);
            }

            _db.OutboxMessages.Remove(outboxMessage);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    private static bool TryToDomainEvent(OutboxMessage outboxMessage, [NotNullWhen(true)] out object? domainEvent)
    {
        var eventType = EventAssembly.GetType(outboxMessage.EventType);
        if (eventType is null)
        {
            domainEvent = null;
            return false;
        }

        domainEvent = JsonSerializer.Deserialize(outboxMessage.EventPayload, eventType);
        return domainEvent is not null;
    }
}
