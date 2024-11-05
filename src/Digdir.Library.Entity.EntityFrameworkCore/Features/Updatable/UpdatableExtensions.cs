using Digdir.Library.Entity.Abstractions.Features.SoftDeletable;
using Digdir.Library.Entity.Abstractions.Features.Updatable;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Digdir.Library.Entity.EntityFrameworkCore.Features.Updatable;

internal static class UpdatableExtensions
{
    public static ModelBuilder AddUpdatableEntities(this ModelBuilder modelBuilder)
    {
        return modelBuilder.EntitiesOfType<IUpdateableEntity>(builder =>
            builder.Property(nameof(IUpdateableEntity.UpdatedAt)).HasDefaultValueSql("current_timestamp at time zone 'utc'"));
    }

    public static ChangeTracker HandleUpdatableEntities(this ChangeTracker changeTracker, DateTimeOffset utcNow)
    {
        var updatableEntities = changeTracker
            .Entries<IUpdateableEntity>()
            .Where(IsNotSoftDeleted)
            .Where(x => AddedWithoutExplicitUpdatedAt(x) || Modified(x));

        foreach (var entity in updatableEntities)
        {
            entity.Entity.Update(utcNow);
        }

        return changeTracker;
    }

    private static bool IsNotSoftDeleted(EntityEntry<IUpdateableEntity> x) => x.Entity is not ISoftDeletableEntity { Deleted: true };
    private static bool AddedWithoutExplicitUpdatedAt(EntityEntry<IUpdateableEntity> x) => x.State is EntityState.Added && x.Entity.UpdatedAt == default;
    private static bool Modified(EntityEntry<IUpdateableEntity> x) => x.State is EntityState.Modified;
}
