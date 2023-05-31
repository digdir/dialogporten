namespace Digdir.Domain.Dialogporten.Domain.Outboxes;

public sealed class OutboxMessageConsumer
{
    public Guid EventId { get; set; }
    public string ConsumerName { get; set; } = string.Empty;

    public OutboxMessage OutboxMessage { get; set; } = null!;
}
