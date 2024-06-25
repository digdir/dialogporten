using Digdir.Domain.Dialogporten.Domain.RoleResources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Configurations.RoleResources;

internal sealed class RoleResourceConfiguration : IEntityTypeConfiguration<RoleResource>
{
    public void Configure(EntityTypeBuilder<RoleResource> builder)
    {
        builder.ToTable("RoleResource");
        builder.HasNoKey();
    }
}
