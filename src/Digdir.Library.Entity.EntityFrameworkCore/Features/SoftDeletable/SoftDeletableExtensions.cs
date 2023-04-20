using Digdir.Library.Entity.Abstractions.Features.SoftDeletable;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Reflection;

namespace Digdir.Library.Entity.EntityFrameworkCore.Features.SoftDeletable;

internal static class SoftDeletableExtensions
{
    private static readonly MethodInfo _openGenericInternalMethodInfo = typeof(SoftDeletableExtensions)
            .GetMethod(nameof(EnableSoftDeletableQueryFilter_Internal), BindingFlags.NonPublic | BindingFlags.Static)!;

    public static ModelBuilder EnableSoftDeletableQueryFilter(this ModelBuilder modelBuilder)
    {
        var deletableEntities = modelBuilder.Model
            .GetEntityTypes()
            .Where(x => typeof(ISoftDeletableEntity).IsAssignableFrom(x.ClrType));

        foreach (var deletableEntity in deletableEntities)
        {
            var method = _openGenericInternalMethodInfo.MakeGenericMethod(deletableEntity.ClrType);
            method.Invoke(null, new object[] { modelBuilder });
        }

        return modelBuilder;
    }

    public static ChangeTracker HandleSoftDeletableEntities(this ChangeTracker changeTracker, Guid userId, DateTimeOffset utcNow)
    {
        var softDeletableEneities = changeTracker
            .Entries<ISoftDeletableEntity>()
            .Where(x => x.State == EntityState.Deleted);

        foreach (var entity in softDeletableEneities)
        {
            entity.Entity.Delete(userId, utcNow);
            // Change to modified so EF will update the soft deleted 
            // entity, instead of hard deleting it.
            entity.State = EntityState.Modified;
        }

        return changeTracker;
    }

    private static void EnableSoftDeletableQueryFilter_Internal<T>(ModelBuilder modelBuilder)
        where T : class, ISoftDeletableEntity
    {
        modelBuilder.Entity<T>().HasQueryFilter(x => !x.Deleted);
    }
}
