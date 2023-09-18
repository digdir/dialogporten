using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Configurations.Actions;

internal sealed class DialogApiActionConfiguration : IEntityTypeConfiguration<DialogApiAction>
{
    public void Configure(EntityTypeBuilder<DialogApiAction> builder)
    {
        builder
            .HasAggregateParent(x => x.Dialog)
            .HasOne(x => x.DialogElement)
            .WithMany(x => x.ApiActions)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
