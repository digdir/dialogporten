using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Contents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Configurations.Dialogs.Content;
internal class DialogContentConfiguration : IEntityTypeConfiguration<DialogContent>
{
    public void Configure(EntityTypeBuilder<DialogContent> builder)
        => builder.HasIndex(x => new { x.DialogId, x.TypeId }).IsUnique();
}

internal class TransmissionContentConfiguration : IEntityTypeConfiguration<TransmissionContent>
{
    public void Configure(EntityTypeBuilder<TransmissionContent> builder)
        => builder.HasIndex(x => new { x.TransmissionId, x.TypeId }).IsUnique();
}
