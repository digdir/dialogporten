using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Configurations.ResourcePolicyMetadata;

internal sealed class ResourcePolicyMetadataConfiguration : IEntityTypeConfiguration<Domain.ResourcePolicyMetadata.ResourcePolicyMetadata>
{
    public void Configure(EntityTypeBuilder<Domain.ResourcePolicyMetadata.ResourcePolicyMetadata> builder)
    {
        builder
            .HasIndex(r => new { r.Resource })
            .IsUnique();
    }
}
