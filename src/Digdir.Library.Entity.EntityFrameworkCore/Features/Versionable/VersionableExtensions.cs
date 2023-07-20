using Digdir.Library.Entity.Abstractions.Features.Versionable;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Digdir.Library.Entity.EntityFrameworkCore.Features.Versionable;

internal static class VersionableExtensions
{
    internal static ModelBuilder AddVersionableEntities(this ModelBuilder modelBuilder)
    {
        return modelBuilder.EntitiesOfType<IVersionableEntity>(builder => builder
            .Property(nameof(IVersionableEntity.ETag))
            .HasDefaultValueSql("gen_random_uuid()")
            .IsConcurrencyToken());
    }

    internal static ChangeTracker HandleVersionableEntities(this ChangeTracker changeTracker)
    {
        var hasChanges = changeTracker.HasChanges();
        foreach (var entry in changeTracker
            .Entries<IVersionableEntity>()
            .Where(x =>
                x.State is EntityState.Added or EntityState.Modified ||
                // Assume that sub entities of the unchanged aggregates/versinable
                // entities are modified when the database has changes.
                x.State is EntityState.Unchanged && hasChanges
            ))
        {
            var etagProp = entry.Property(x => x.ETag);
            if (etagProp.OriginalValue == etagProp.CurrentValue)
            {
                entry.Entity.NewVersion();
            }
        }

        return changeTracker;
    }
}