using Digdir.Domain.Dialogporten.Domain.Dialogues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Configurations.Dialogues;

internal sealed class DialogueEntityConfiguration : IEntityTypeConfiguration<DialogueEntity>
{
    public void Configure(EntityTypeBuilder<DialogueEntity> builder)
    {

    }
}
