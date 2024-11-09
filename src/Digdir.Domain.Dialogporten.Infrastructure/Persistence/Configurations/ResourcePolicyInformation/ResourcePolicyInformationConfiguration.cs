using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Configurations.ResourcePolicyInformation;

internal sealed class ResourcePolicyInformationConfiguration : IEntityTypeConfiguration<Domain.ResourcePolicyInformation.ResourcePolicyInformation>
{
    public void Configure(EntityTypeBuilder<Domain.ResourcePolicyInformation.ResourcePolicyInformation> builder)
    {
        builder
            .HasIndex(r => new { r.Resource })
            .IsUnique();
    }
}
