using Digdir.Library.Entity.Abstractions.Features.Creatable;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Digdir.Library.Entity.EntityFrameworkCore.Features.Creatable;

internal static class CreatableExtensions
{
    public static ModelBuilder AddCreatableEntities(this ModelBuilder modelBuilder)
    {
        return modelBuilder.EntitiesOfType<ICreatableEntity>(builder =>
            builder.Property(nameof(ICreatableEntity.CreatedAtUtc)).HasDefaultValueSql("current_timestamp at time zone 'utc'"));
    }

    public static ChangeTracker HandleCreatableEntities(this ChangeTracker changeTracker, Guid userId, DateTime utcNow)
    {
        var creatableEntities = changeTracker
            .Entries<ICreatableEntity>()
            .Where(x => x.State == EntityState.Added);

        foreach (var entity in creatableEntities)
        {
            entity.Entity.Create(userId, utcNow);
        }

        return changeTracker;
    }
}
