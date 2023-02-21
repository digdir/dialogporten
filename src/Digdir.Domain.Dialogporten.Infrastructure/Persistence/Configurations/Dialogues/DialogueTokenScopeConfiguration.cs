using Digdir.Domain.Dialogporten.Domain.Dialogues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Configurations.Dialogues;

internal sealed class DialogueTokenScopeConfiguration : IEntityTypeConfiguration<DialogueTokenScope>
{
    public void Configure(EntityTypeBuilder<DialogueTokenScope> builder)
    {
        builder.HasIndex(x => new { x.DialogueId, x.Value }).IsUnique();
    }
}
