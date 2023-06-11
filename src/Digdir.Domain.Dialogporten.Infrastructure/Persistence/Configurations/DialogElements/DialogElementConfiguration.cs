using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Configurations.DialogElements;

internal sealed class DialogElementConfiguration : IEntityTypeConfiguration<DialogElement>
{
    public void Configure(EntityTypeBuilder<DialogElement> builder)
    {
        builder
            .HasOne(e => e.RelatedDialogElement)
            .WithMany()
            .HasForeignKey(e => e.RelatedDialogElementInternalId);
    }
}
