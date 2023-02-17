using Digdir.Domain.Dialogporten.Domain.Dialogues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Configurations.Dialogues;

internal sealed class DialogueContentConfiguration : IEntityTypeConfiguration<DialogueContent>
{
    public void Configure(EntityTypeBuilder<DialogueContent> builder)
    {
        // TODO: er det korrekt delete behavior her? 
        builder.HasOne(x => x.SenderName).WithOne()
            .HasForeignKey<DialogueContent>($"{nameof(DialogueContent.SenderName)}InternalId")
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Title).WithOne()
            .HasForeignKey<DialogueContent>($"{nameof(DialogueContent.Title)}InternalId")
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Body).WithOne()
            .HasForeignKey<DialogueContent>($"{nameof(DialogueContent.Body)}InternalId")
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.SearchTitle).WithOne()
            .HasForeignKey<DialogueContent>($"{nameof(DialogueContent.SearchTitle)}InternalId")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
