using Digdir.Domain.Dialogporten.Domain.Common;
using Digdir.Domain.Dialogporten.Infrastructure.DomainEvents.Outbox.Entities;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;

namespace Digdir.Domain.Dialogporten.Infrastructure.DomainEvents.Outbox.Dispatcher;

internal sealed class OutboxDispatcher
{
    private static readonly AsyncRetryPolicy _retryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(3,
            attempt => TimeSpan.FromMilliseconds(50 * attempt));

    private readonly DialogueDbContext _db;
    private readonly IPublisher _publisher;

    public OutboxDispatcher(DialogueDbContext db, IPublisher publisher)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }

    public async Task Execute(CancellationToken cancellationToken = default)
    {
        var outboxMessages = await _db
            .Set<OutboxMessage>()
            .Where(x => x.Status == OutboxStatus.Unprocessed)
            .OrderBy(x => x.LastAttemptedAtUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var outboxMessage in outboxMessages)
        {
            if (!outboxMessage.TryParseDomainEvent<IDomainEvent>(out var domainEvent))
            {
                // TODO: Handle better
                outboxMessage.Discard();
                continue;
            }

            var result = await _retryPolicy
                .ExecuteAndCaptureAsync(
                    ct => _publisher.Publish(domainEvent, ct),
                    cancellationToken);

            if (result.Outcome == OutcomeType.Failure)
            {
                outboxMessage.Failure(result.FinalException.Message);
                continue;
            }

            outboxMessage.Success();
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
