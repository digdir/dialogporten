using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;
using Digdir.Domain.Dialogporten.Infrastructure.Common;
using Digdir.Library.Entity.EntityFrameworkCore.Features.Aggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Configurations.DialogElements;

internal sealed class DialogElementConfiguration : IEntityTypeConfiguration<DialogElement>
{
    public void Configure(EntityTypeBuilder<DialogElement> builder)
    {
        builder.HasAggregateParent(x => x.Dialog)
            .HasOne(x => x.RelatedDialogElement)
            .WithMany(x => x.RelatedDialogElements)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

internal sealed class DialogElementUrlConfiguration : IEntityTypeConfiguration<DialogElementUrl>
{
    public void Configure(EntityTypeBuilder<DialogElementUrl> builder)
    {
        builder.HasAggregateParent(x => x.DialogElement);
    }
}

internal sealed class DialogElementDisplayNameConfiguration : IEntityTypeConfiguration<DialogElementDisplayName>
{
    public void Configure(EntityTypeBuilder<DialogElementDisplayName> builder)
    {
        builder.HasAggregateParent(x => x.Element);
    }
}
