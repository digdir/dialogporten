namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Outbox;

internal sealed class OutboxMessageConsumer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
