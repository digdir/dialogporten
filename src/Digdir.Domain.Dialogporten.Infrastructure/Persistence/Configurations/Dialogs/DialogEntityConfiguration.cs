using Digdir.Domain.Dialogporten.Domain.Common;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Configurations.Dialogs;

internal sealed class DialogEntityConfiguration : IEntityTypeConfiguration<DialogEntity>
{
    public void Configure(EntityTypeBuilder<DialogEntity> builder)
    {
        builder.ToTable("Dialog");
        builder.Property(x => x.ServiceResource)
            .HasMaxLength(Constants.DefaultMaxStringLength);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.DueAt);
        builder.HasIndex(x => x.UpdatedAt);
        builder.Property(x => x.Org).UseCollation("C");
        builder.Property(x => x.ServiceResource).UseCollation("C");
        builder.Property(x => x.Party).UseCollation("C");
        builder.HasIndex(x => x.Org);
        builder.HasIndex(x => x.ServiceResource);
        builder.HasIndex(x => x.Party);
    }
}
