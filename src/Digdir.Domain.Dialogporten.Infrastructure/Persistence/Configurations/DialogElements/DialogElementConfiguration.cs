using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Configurations.DialogElements;

internal sealed class DialogElementConfiguration : IEntityTypeConfiguration<DialogElement>
{
    public void Configure(EntityTypeBuilder<DialogElement> builder)
    {
        builder.HasOne(x => x.RelatedDialogElement)
            .WithMany(x => x.RelatedDialogElements)
            .OnDelete(DeleteBehavior.SetNull);
    }
}