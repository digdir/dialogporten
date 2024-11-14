using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Configurations.ResourcePolicy;

internal sealed class ResourcePolicyConfiguration : IEntityTypeConfiguration<Domain.ResourcePolicy.ResourcePolicy>
{
    public void Configure(EntityTypeBuilder<Domain.ResourcePolicy.ResourcePolicy> builder)
    {
        builder
            .HasIndex(r => new { r.Resource })
            .IsUnique();
    }
}
