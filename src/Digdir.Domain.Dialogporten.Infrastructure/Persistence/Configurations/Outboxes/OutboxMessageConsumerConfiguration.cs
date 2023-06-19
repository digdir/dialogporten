using Digdir.Domain.Dialogporten.Domain.Outboxes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Configurations.Outboxes;

internal sealed class OutboxMessageConsumerConfiguration : IEntityTypeConfiguration<OutboxMessageConsumer>
{
    public void Configure(EntityTypeBuilder<OutboxMessageConsumer> builder)
    {
        builder.HasKey(x => new { x.EventId, x.ConsumerName });
        builder.HasOne(x => x.OutboxMessage)
            .WithMany(x => x.OutboxMessageConsumers)
            .HasForeignKey(x => x.EventId);
    }
}
