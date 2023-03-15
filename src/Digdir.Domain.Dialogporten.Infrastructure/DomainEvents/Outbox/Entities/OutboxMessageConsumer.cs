namespace Digdir.Domain.Dialogporten.Infrastructure.DomainEvents.Outbox.Entities;

internal sealed class OutboxMessageConsumer
{
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
}
