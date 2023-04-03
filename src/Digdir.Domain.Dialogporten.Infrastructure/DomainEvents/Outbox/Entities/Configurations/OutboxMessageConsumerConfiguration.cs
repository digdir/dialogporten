using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Digdir.Domain.Dialogporten.Infrastructure.DomainEvents.Outbox.Entities.Configurations;

internal sealed class OutboxMessageConsumerConfiguration : IEntityTypeConfiguration<OutboxMessageConsumer>
{
    public void Configure(EntityTypeBuilder<OutboxMessageConsumer> builder)
    {
        builder.HasKey(x => new { x.EventId, x.Name });
        builder.HasOne<OutboxMessage>().WithMany().HasForeignKey(x => x.EventId);
    }
}
