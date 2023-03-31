using Digdir.Domain.Dialogporten.Domain.Common;
using Digdir.Domain.Dialogporten.Infrastructure.Common.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Digdir.Domain.Dialogporten.Infrastructure.DomainEvents.Outbox.Entities;

internal sealed class OutboxMessage
{
    private const int MaxNumberOfAttempts = 10;

    public Guid EventId { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public int NumberOfAttempts { get; private set; }

    public string Content { get; private set; } = string.Empty;
    public OutboxStatus Status { get; private set; }
    public string? Error { get; private set; }
    public DateTimeOffset? LastAttemptedAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private OutboxMessage() { }

    public static OutboxMessage Create(IDomainEvent domainEvent, DateTimeOffset createdAtUtc) => new()
    {
        EventId = domainEvent.EventId,
        CreatedAtUtc = createdAtUtc,
        Type = domainEvent.GetType().Name,
        Content = JsonSerializer.Serialize(domainEvent, JsonSerializerExtensions.DomainEventPolymorphismOptions),
        Status = OutboxStatus.Unprocessed
    };

    public bool TryParseDomainEvent<TDomainEvent>(
        [NotNullWhen(true)] out TDomainEvent? domainEvent)
        where TDomainEvent : IDomainEvent
    {
        domainEvent = (TDomainEvent?)JsonSerializer
            .Deserialize<IDomainEvent>(Content,
                JsonSerializerExtensions.DomainEventPolymorphismOptions);
        return domainEvent is not null;
    }

    public void Success()
    {
        LastAttemptedAtUtc = DateTimeOffset.UtcNow;
        NumberOfAttempts++;
        Status = OutboxStatus.Processed;
    }

    public void Failure(string error)
    {
        Error = error;
        LastAttemptedAtUtc = DateTimeOffset.UtcNow;
        NumberOfAttempts++;
        if (NumberOfAttempts >= MaxNumberOfAttempts)
        {
            Discard();
        }
    }

    public void Discard()
    {
        Status = OutboxStatus.Discarded;
    }
}

internal enum OutboxStatus
{
    Unprocessed,
    Processed,
    Discarded
}