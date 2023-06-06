using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Configurations.Dialogs;

internal sealed class DialogEntityConfiguration : IEntityTypeConfiguration<DialogEntity>
{
    public void Configure(EntityTypeBuilder<DialogEntity> builder)
    {
        builder.ToTable("Dialog");

        // TODO: er det korrekt delete behavior her? 
        builder.HasOne(x => x.SenderName).WithOne()
            .HasPrincipalKey<LocalizationSet>(x => x.InternalId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Title).WithOne()
            .HasPrincipalKey<LocalizationSet>(x => x.InternalId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Body).WithOne()
            .HasPrincipalKey<LocalizationSet>(x => x.InternalId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.SearchTitle).WithOne()
            .HasPrincipalKey<LocalizationSet>(x => x.InternalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
