﻿using Digdir.Library.Entity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Digdir.Library.Entity.Abstractions.Features.Concurrency;

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
        foreach (var entry in changeTracker
            .Entries<IVersionableEntity>()
            .Where(x => x.State is EntityState.Added or EntityState.Modified))
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