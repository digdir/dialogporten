using Digdir.Domain.Dialogporten.Domain.Common;
using System.Text.Json;

namespace Digdir.Domain.Dialogporten.Domain.Outboxes;

public sealed class OutboxMessage : IDisposable
{
    public Guid EventId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public JsonDocument EventPayload { get; private set; } = null!;

    //private OutboxMessage() { }

    public static OutboxMessage Create<TDomainEvent>(TDomainEvent domainEvent)
        where TDomainEvent : IDomainEvent => new()
        {
            EventId = domainEvent.EventId,
            EventType = domainEvent.GetType().FullName!,
            EventPayload = JsonSerializer.SerializeToDocument<object>(domainEvent)
        };

    public void Dispose() => EventPayload?.Dispose();

    //public bool TryParseDomainEvent<TDomainEvent>([NotNullWhen(true)] out TDomainEvent? domainEvent)
    //    where TDomainEvent : IDomainEvent
    //{
    //    domainEvent = (TDomainEvent?)JsonSerializer
    //        .Deserialize<IDomainEvent>(EventPeyload,
    //            JsonSerializerExtensions.DomainEventPolymorphismOptions);
    //    return domainEvent is not null;
    //}
}