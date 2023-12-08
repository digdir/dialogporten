using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Configurations.Dialogs.Actions;

internal sealed class DialogApiActionConfiguration : IEntityTypeConfiguration<DialogApiAction>
{
    public void Configure(EntityTypeBuilder<DialogApiAction> builder)
    {
        builder.HasOne(x => x.DialogElement)
            .WithMany(x => x.ApiActions)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
