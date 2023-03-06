using Digdir.Domain.Dialogporten.Domain;
using Digdir.Domain.Dialogporten.Infrastructure.Common.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using System.Text.Json;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Outbox;

internal sealed class ProcessOutboxMessagesJob
{
    private static readonly AsyncRetryPolicy _retryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(3, 
            attempt => TimeSpan.FromMilliseconds(50 * attempt));

    private readonly DialogueDbContext _db;
    private readonly IPublisher _publisher;

    public ProcessOutboxMessagesJob(DialogueDbContext db, IPublisher publisher)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }

    public async Task Execute(CancellationToken cancellationToken = default)
    {
        var messages = await _db
            .Set<OutboxMessage>()
            .Where(x => x.ProcessedAtUtc == null &&
                        x.Error == null)
            .OrderBy(x => x.ProcessedAtUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var outboxMessage in messages)
        {
            var domainEvent = JsonSerializer.Deserialize<IDomainEvent>(
                outboxMessage.Content, 
                JsonSerializerExtensions.DomainEventPolymorphismOptions);

            if (domainEvent is null)
            {
                // TODO: Handle better
                continue;
            }

            // TODO: Lage egne scope til hver handler?
            var result = await _retryPolicy
                .ExecuteAndCaptureAsync(
                    ct => _publisher.Publish(domainEvent, ct), 
                    cancellationToken);

            outboxMessage.Error = result.FinalException?.ToString();
            outboxMessage.ProcessedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
