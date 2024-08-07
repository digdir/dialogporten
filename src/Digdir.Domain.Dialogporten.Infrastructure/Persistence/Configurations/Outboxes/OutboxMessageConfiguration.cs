﻿using Digdir.Domain.Dialogporten.Domain.Outboxes;
using Digdir.Library.Entity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Configurations.Outboxes;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(x => x.EventId);
        builder.Property(x => x.EventPayload)
            .HasUnlimitedLength()
            .HasColumnType("jsonb");
        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("current_timestamp at time zone 'utc'");
        builder.Property(x => x.CorrelationId)
            .HasDefaultValueSql("gen_random_uuid()");
    }
}
