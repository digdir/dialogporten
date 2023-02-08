using Digdir.Library.Entity.Abstractions.Features.Updatable;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Digdir.Library.Entity.EntityFrameworkCore.Features.Updatable;

internal static class UpdatableExtensions
{
    public static ModelBuilder AddUpdatableEntities(this ModelBuilder modelBuilder)
    {
        return modelBuilder.EntitiesOfType<IUpdateableEntity>(builder =>
            builder.Property(nameof(IUpdateableEntity.UpdatedAtUtc)).HasDefaultValueSql("GETUTCDATE()"));
    }

    public static ChangeTracker HandleUpdatableEntities(this ChangeTracker changeTracker, Guid userId, DateTime utcNow)
    {
        var updatableEntities = changeTracker.Entries()
            .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified)
            .Where(x => x.Entity is IUpdateableEntity);

        foreach (var entity in updatableEntities)
        {
            var updatable = (IUpdateableEntity)entity.Entity;
            updatable.Update(userId, utcNow);
        }

        return changeTracker;
    }
}
