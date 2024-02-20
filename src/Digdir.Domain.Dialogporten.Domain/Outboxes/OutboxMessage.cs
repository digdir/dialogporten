using System.Text.Json;
using Digdir.Library.Entity.Abstractions.Features.EventPublisher;

namespace Digdir.Domain.Dialogporten.Domain.Outboxes;

public sealed class OutboxMessage
{
    public required Guid EventId { get; init; }
    public required string EventType { get; init; }
    public required string EventPayload { get; init; }

    public List<OutboxMessageConsumer> OutboxMessageConsumers { get; set; } = [];

    public static OutboxMessage Create<TDomainEvent>(TDomainEvent domainEvent)
        where TDomainEvent : IDomainEvent
    {
        var eventType = domainEvent.GetType();
        return new()
        {
            EventId = domainEvent.EventId,
            EventType = eventType.FullName!,
            EventPayload = JsonSerializer.Serialize(domainEvent, eventType)
        };
    }
}
