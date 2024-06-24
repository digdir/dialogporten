using Digdir.Domain.Dialogporten.Domain.SubjectResources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Configurations.SubjectResources;

public class SubjectResourceConfiguration : IEntityTypeConfiguration<SubjectResource>
{
    public void Configure(EntityTypeBuilder<SubjectResource> builder)
    {
        builder.HasKey(sr => new { sr.SubjectId, sr.ResourceId });

        builder.HasOne(sr => sr.Subject)
            .WithMany(s => s.SubjectResources)
            .HasForeignKey(sr => sr.SubjectId);

        builder.HasOne(sr => sr.Resource)
            .WithMany(r => r.SubjectResources)
            .HasForeignKey(sr => sr.ResourceId);
    }
}
