using Digdir.Library.Entity.Abstractions.Features.Creatable;
using Digdir.Library.Entity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Digdir.Library.Entity.EntityFrameworkCore.Features.Creatable;

internal static class CreatableExtensions
{
    public static ModelBuilder AddCreatableEntities(this ModelBuilder modelBuilder)
    {
        return modelBuilder.EntitiesOfType<ICreatableEntity>(builder =>
            builder.Property(nameof(ICreatableEntity.CreatedAtUtc)).HasDefaultValueSql("GETUTCDATE()"));
    }

    public static ChangeTracker HandleCreatableEntities(this ChangeTracker changeTracker, Guid userId, DateTime utcNow)
    {
        var creatableEntities = changeTracker.Entries()
            .Where(x => x.State == EntityState.Added)
            .Where(x => x.Entity is ICreatableEntity);

        foreach (var entity in creatableEntities)
        {
            var creatable = (ICreatableEntity)entity.Entity;
            creatable.Create(userId, utcNow);
        }

        return changeTracker;
    }
}
