using Digdir.Domain.Dialogporten.Domain.SubjectResources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Configurations.SubjectResources;

internal sealed class SubjectResourceLastUpdateConfiguration : IEntityTypeConfiguration<SubjectResourceLastUpdate>
{
    public void Configure(EntityTypeBuilder<SubjectResourceLastUpdate> builder)
    {
        builder.ToTable("SubjectResourceLastUpdate");
        builder.HasNoKey();
    }
}
