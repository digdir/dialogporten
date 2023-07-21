using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using Digdir.Library.Entity.EntityFrameworkCore;
using Digdir.Library.Entity.Abstractions.Features.Updatable;
using Digdir.Library.Entity.Abstractions.Features.Versionable;

namespace Digdir.Library.Entity.Abstractions.Features.Hierarchy;

internal static class SubEntityExtensions
{
    public static ModelBuilder AddSubEntities(this ModelBuilder modelBuilder)
    {
        return modelBuilder.EntitiesOfType<ISubEntity>(builder => builder.Ignore(nameof(ISubEntity.Parent)));
    }

    public static ChangeTracker HandleSubEntities(this ChangeTracker changeTracker, DateTimeOffset utcNow)
    {
        var parentEntities = changeTracker
            .Entries<ISubEntity>()
            .Where(x => x.State == EntityState.Modified)
            .SelectMany(x => x.Entity.ParentHierarchy())
            .Distinct();

        foreach (var entity in parentEntities)
        {
            if (entity is IUpdateableEntity updatable)
            {
                updatable.Update(utcNow);
            }

            if (entity is IVersionableEntity versionable)
            {
                versionable.NewVersion();
            }
        }

        return changeTracker;
    }

    private static IEnumerable<IEntityBase> ParentHierarchy(this ISubEntity entity)
    {
        for (var parent = entity.Parent; parent is not null; parent = parent is ISubEntity subParent ? subParent.Parent : null)
        {
            yield return parent;
        }
    }
}
