// using System.Diagnostics;
// using System.Linq.Expressions;
// using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.ChangeTracking;
// using Microsoft.EntityFrameworkCore.Infrastructure;
// using Microsoft.EntityFrameworkCore.Metadata;
// using Microsoft.EntityFrameworkCore.Metadata.Builders;
//
// namespace Digdir.Domain.Dialogporten.Infrastructure.Common;
//
// public static class ModelBuilderExtensions
// {
//     public static EntityTypeBuilder<TEntity> HasAggregateParent<TEntity, TProperty>(
//         this EntityTypeBuilder<TEntity> entityTypeBuilder,
//         Expression<Func<TEntity, TProperty>> navigationProperty)
//         where TEntity : class
//     {
//         var foreignKey = entityTypeBuilder
//             .Metadata
//             .FindNavigation(navigationProperty.GetMemberAccess())
//             .ForeignKey;
//         foreignKey.AddAnnotation("DddAggregateParent", null);
//         
//         return entityTypeBuilder;
//     }
//
//     public static IEnumerable<IForeignKey> FindAggregateParents(this IEntityType entityType)
//     {
//         return entityType
//             .GetForeignKeys()
//             .Where(x => x.FindAnnotation("DddAggregateParent") is not null);
//     }
// }
//
// public static class ChangeTrackerExtensions
// {
//     private static void NoOp(object? child, DateTimeOffset utcNow) { }
//
//     public static async Task Something(this ChangeTracker changeTracker, DateTimeOffset utcNow)
//     {
//         var modifiedEntries = changeTracker
//             .Entries()
//             .Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
//             .ToList();
//
//         var foo = new List<(EntityEntry Entry, EntityEntry? Child)>();
//         var bar = new Dictionary<object, HashSet<object>>();
//         
//         await foreach (var (entry, child) in modifiedEntries.GetOwnershipChainInclusive())
//         {
//             foo.Add((entry, child));
//             if (entry.Entity is not INotifyChange nc) continue;
//             
//             var @delegate = entry.State switch
//             {
//                 EntityState.Unchanged => nc.Updated,
//                 EntityState.Modified => nc.Updated,
//                 EntityState.Deleted => nc.Deleted,
//                 EntityState.Added => nc.Created,
//                 EntityState.Detached => (Action<object?, DateTimeOffset>) NoOp,
//                 _ => throw new UnreachableException($"Unknown {nameof(EntityState)}. Got {entry.State}."),
//             };
//
//             // @delegate(child?.Entity, utcNow);
//         }
//         
//     }
//
//     private static async IAsyncEnumerable<(EntityEntry Entry, EntityEntry? Child)> GetOwnershipChainInclusive(this IEnumerable<EntityEntry> entries)
//     {
//         var seenEntities = new HashSet<(object, object?)>();
//         foreach (var entry in entries)
//         {
//             await foreach (var (currentEntry, currentChild) in entry.GetOwnershipChainInclusive())
//             {
//                 // If I've already seen this pair, I've seen it's owner chain as well. 
//                 if (!seenEntities.Add((currentEntry.Entity, currentChild?.Entity)))
//                 {
//                     break;
//                 }
//
//                 yield return (currentEntry, currentChild);
//             }
//         }
//     }
//
//     private static async IAsyncEnumerable<(EntityEntry Entry, EntityEntry? Child)> GetOwnershipChainInclusive(
//         this EntityEntry entry, 
//         HashSet<object>? visitedEntities = null)
//     {
//         if (visitedEntities is null)
//         {
//             visitedEntities = new();
//             // Assume first call, return the first entry (the inclusive part of the method)
//             yield return (entry, null);
//         }
//
//         if (!visitedEntities.Add(entry.Entity))
//         {
//             yield break; // Skip already visited entries
//         }
//
//         foreach (var parentForeignKey in entry.Metadata.FindAggregateParents())
//         {
//             var parentType = parentForeignKey.PrincipalEntityType.ClrType;
//             var parentPrimaryKey = parentForeignKey.Properties
//                 .Select(key => entry.OriginalValues[key.Name])
//                 .ToArray();
//
//             if (parentPrimaryKey.All(x => x is null))
//             {
//                 continue;
//             }
//
//             var parentEntity = await entry.Context.FindAsync(parentType, parentPrimaryKey);
//
//             if (parentEntity is null)
//             {
//                 continue;
//             }
//
//             var parentEntry = entry.Context.Entry(parentEntity);
//
//             yield return (parentEntry, entry);
//
//             await foreach (var parentAndGrandparent in GetOwnershipChainInclusive(parentEntry, visitedEntities))
//             {
//                 yield return parentAndGrandparent;
//             }
//         }
//     }
// }
//

using System.Diagnostics;
using System.Linq.Expressions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Digdir.Domain.Dialogporten.Infrastructure.Common;

public static class ModelBuilderExtensions
{
    public static EntityTypeBuilder<TEntity> HasAggregateParent<TEntity, TProperty>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        Expression<Func<TEntity, TProperty>> navigationProperty)
        where TEntity : class
    {
        var foreignKey = entityTypeBuilder
            .Metadata
            .FindNavigation(navigationProperty.GetMemberAccess())
            .ForeignKey;
        foreignKey.AddAnnotation("DddAggregateParent", null);
        
        return entityTypeBuilder;
    }

    public static IEnumerable<IForeignKey> FindAggregateParents(this IEntityType entityType)
    {
        return entityType
            .GetForeignKeys()
            .Where(x => x.FindAnnotation("DddAggregateParent") is not null);
    }
}

public static class ChangeTrackerExtensions
{
    private static void NoOp(object? child, DateTimeOffset utcNow) { }

    public static async Task Something(this ChangeTracker changeTracker, DateTimeOffset utcNow)
    {
        var modifiedEntries = changeTracker
            .Entries()
            .Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        var foo = new List<(EntityEntry Entry, EntityEntry? Child)>();
        var bar = new Dictionary<object, HashSet<object>>();
        
        await foreach (var (entry, child) in modifiedEntries.GetOwnershipChainInclusive())
        {
            foo.Add((entry, child));
            if (entry.Entity is not INotifyChange nc) continue;
            
            var @delegate = entry.State switch
            {
                EntityState.Unchanged => nc.Updated,
                EntityState.Modified => nc.Updated,
                EntityState.Deleted => nc.Deleted,
                EntityState.Added => nc.Created,
                EntityState.Detached => (Action<object?, DateTimeOffset>) NoOp,
                _ => throw new UnreachableException($"Unknown {nameof(EntityState)}. Got {entry.State}."),
            };

            // @delegate(child?.Entity, utcNow);
        }
        
    }

    private static async IAsyncEnumerable<(EntityEntry Entry, EntityEntry? Child)> GetOwnershipChainInclusive(this IEnumerable<EntityEntry> entries)
    {
        var seenEntities = new HashSet<(object, object?)>();
        foreach (var entry in entries)
        {
            await foreach (var (currentEntry, currentChild) in entry.GetOwnershipChainInclusive())
            {
                // If I've already seen this pair, I've seen it's owner chain as well. 
                if (!seenEntities.Add((currentEntry.Entity, currentChild?.Entity)))
                {
                    break;
                }

                yield return (currentEntry, currentChild);
            }
        }
    }

    private static async IAsyncEnumerable<(EntityEntry Entry, EntityEntry? Child)> GetOwnershipChainInclusive(
        this EntityEntry entry, 
        HashSet<object>? visitedEntities = null)
    {
        if (visitedEntities is null)
        {
            visitedEntities = new();
            // Assume first call, return the first entry (the inclusive part of the method)
            yield return (entry, null);
        }

        if (!visitedEntities.Add(entry.Entity))
        {
            yield break; // Skip already visited entries
        }

        foreach (var parentForeignKey in entry.Metadata.FindAggregateParents())
        {
            var parentType = parentForeignKey.PrincipalEntityType.ClrType;
            var parentPrimaryKey = parentForeignKey.Properties
                .Select(key => entry.OriginalValues[key.Name])
                .ToArray();

            if (parentPrimaryKey.All(x => x is null))
            {
                continue;
            }

            var parentEntity = await entry.Context.FindAsync(parentType, parentPrimaryKey);

            if (parentEntity is null)
            {
                continue;
            }

            var parentEntry = entry.Context.Entry(parentEntity);

            yield return (parentEntry, entry);

            await foreach (var parentAndGrandparent in GetOwnershipChainInclusive(parentEntry, visitedEntities))
            {
                yield return parentAndGrandparent;
            }
        }
    }
}
