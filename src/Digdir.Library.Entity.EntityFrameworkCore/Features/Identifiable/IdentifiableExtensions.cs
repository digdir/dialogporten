using Digdir.Library.Entity.Abstractions.Features.Identifiable;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Digdir.Library.Entity.EntityFrameworkCore.Features.Identifiable;

internal static class IdentifiableExtensions
{
    public static ModelBuilder AddIdentifiableEntities(this ModelBuilder modelBuilder)
    {
        return modelBuilder.EntitiesOfType<IIdentifiableEntity>(builder =>
        {
            builder.Property(nameof(IIdentifiableEntity.Id)).HasDefaultValueSql("NEWID()");
            builder.HasIndex(nameof(IIdentifiableEntity.Id)).IsUnique();
            builder.HasKey(nameof(IIdentifiableEntity.InternalId));
        });
    }

    public static ChangeTracker HandleIdentifiableEntities(this ChangeTracker changeTracker)
    {
        var identifiableEntities = changeTracker.Entries()
            .Where(x => x.State == EntityState.Added)
            .Where(x => x.Entity is IIdentifiableEntity);

        foreach (var entity in identifiableEntities)
        {
            var identifiable = (IIdentifiableEntity)entity.Entity;
            identifiable.CreateId();
        }

        return changeTracker;
    }
}
