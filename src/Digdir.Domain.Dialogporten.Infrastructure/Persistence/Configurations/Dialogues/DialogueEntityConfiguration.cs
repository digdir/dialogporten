using Digdir.Domain.Dialogporten.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Configurations.Dialogues;

internal sealed class DialogueEntityConfiguration : IEntityTypeConfiguration<DialogueEntity>
{
    public void Configure(EntityTypeBuilder<DialogueEntity> builder)
    {

    }
}
