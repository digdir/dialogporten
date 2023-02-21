using Digdir.Domain.Dialogporten.Domain.Dialogues;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Configurations.Dialogues;

internal sealed class DialogueEntityConfiguration : IEntityTypeConfiguration<DialogueEntity>
{
    public void Configure(EntityTypeBuilder<DialogueEntity> builder)
    {
        builder.ToTable("Dialogue");

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
