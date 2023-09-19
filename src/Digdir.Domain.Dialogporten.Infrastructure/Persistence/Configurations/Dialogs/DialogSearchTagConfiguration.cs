using Digdir.Domain.Dialogporten.Domain.Common;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Infrastructure.Common;
using Digdir.Library.Entity.EntityFrameworkCore.Features.Aggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Configurations.Dialogs;

internal sealed class DialogSearchTagConfiguration : IEntityTypeConfiguration<DialogSearchTag>
{
    public void Configure(EntityTypeBuilder<DialogSearchTag> builder)
    {
        builder.HasAggregateParent(x => x.Dialog);
        builder.HasIndex(x => new {x.DialogId, x.Value})
            .IsUnique();
        builder.Property(x => x.Value)
            .HasMaxLength(Constants.MaxSearchTagLength);
    }
}