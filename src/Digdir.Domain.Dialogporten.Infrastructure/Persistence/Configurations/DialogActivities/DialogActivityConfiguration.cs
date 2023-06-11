using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Configurations.DialogActivities;

internal sealed class DialogActivityConfiguration : IEntityTypeConfiguration<DialogActivity>
{
    public void Configure(EntityTypeBuilder<DialogActivity> builder)
    {
        builder
            .HasOne(e => e.RelatedActivity)
            .WithMany()
            .HasForeignKey(e => e.RelatedActivityInternalId);

        builder
            .HasOne(e => e.DialogElement)
            .WithMany()
            .HasForeignKey(e => e.DialogElementInternalId);
    }
}
