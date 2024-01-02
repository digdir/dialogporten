using System.Collections;
using System.Diagnostics;
using System.Reflection;
using Digdir.Library.Entity.Abstractions.Features.Aggregate;
using Digdir.Library.Entity.Abstractions.Features.Creatable;
using Digdir.Library.Entity.Abstractions.Features.Updatable;
using Digdir.Library.Entity.Abstractions.Features.Versionable;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Digdir.Library.Entity.EntityFrameworkCore.Features.Aggregate;

internal static class AggregateExtensions
{
    private static readonly EntityEntryComparer _entityEntryComparer = new();

    internal static async Task HandleAggregateEntities(this ChangeTracker changeTracker,
        DateTimeOffset utcNow, CancellationToken cancellationToken)
    {
        var aggregateNodeByEntry = await changeTracker
            .Entries()
            .Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .GetAggregateNodeByEntry(cancellationToken);

        foreach (var (entry, aggregateNode) in aggregateNodeByEntry)
        {
            if (entry.Entity is IAggregateCreatedHandler created && entry.State is EntityState.Added)
            {
                created.OnCreate(aggregateNode, utcNow);
            }

            if (entry.Entity is IAggregateUpdatedHandler updated &&
                entry.State is EntityState.Modified or EntityState.Unchanged)
            {
                updated.OnUpdate(aggregateNode, utcNow);
            }

            if (entry.Entity is IAggregateDeletedHandler deleted && entry.State is EntityState.Deleted)
            {
                deleted.OnDelete(aggregateNode, utcNow);
            }

            if (entry.Entity is IUpdateableEntity updatable)
            {
                updatable.Update(utcNow);
            }

            if (entry.Entity is IVersionableEntity versionable)
            {
                versionable.NewVersion();
            }
        }
    }

    internal static ModelBuilder AddAggregateEntities(this ModelBuilder modelBuilder)
    {
        var entities = modelBuilder.Model
            .GetEntityTypes()
            .Where(x => x.BaseType is null)
            .SelectMany(entityType => entityType
                .FindAggregateChildren()
                .Select(foreignKey => entityType
                    .FindNavigation(foreignKey.PrincipalToDependent!.Name)!));

        foreach (var entityType in entities)
        {
            entityType.SetIsEagerLoaded(true);
        }

        return modelBuilder;
    }

    /* - oppdater oppover
     * - slett nedover
     * - Autoinclude children DONE
     * - Skille på modifisert som del av agregatet eller direkte
     * - Skille på slettet som del av agregatet eller direkte
     * 
     * 
     */

    private static async Task<IReadOnlyDictionary<EntityEntry, AggregateNode>> GetAggregateNodeByEntry(
        this IEnumerable<EntityEntry> entries,
        CancellationToken cancellationToken)
    {
        var nodeByEntry = new Dictionary<EntityEntry, AggregateNode>(comparer: _entityEntryComparer);

        foreach (var entry in entries)
        {
            await nodeByEntry.AddAggregateChain(entry, cancellationToken);
        }

        foreach (var rootNode in nodeByEntry
            .Select(x => x.Value)
            .Where(x => x.IsRootNode))
        {
            rootNode.CollapseAggregateState();
        }

        return nodeByEntry;
    }

    /// <summary>
    /// Will aggregate modified state from children to parent, and deleted state from parent to children. Deleted state from parent to children will take precedence.
    /// </summary>
    /// <param name="node">The AggregateNode whose state is to be collapsed.</param>
    /// <param name="parentState"></param>
    /// <returns>
    ///     <c>true</c> if the node is changed - that is, the node's state is not <see cref="AggregateNodeState.Unchanged"/> after collapsing;
    ///     otherwise, <c>false</c>.
    /// </returns>
    private static bool CollapseAggregateState(this AggregateNode node, AggregateNodeState? parentState = null)
    {
        if (parentState is AggregateNodeState.Deleted)
        {
            node.State = AggregateNodeState.Deleted;
        }

        var childrenIsModified = node.Children
            .Aggregate(false, (previousChildIsModified, childNode) =>
                childNode.CollapseAggregateState(node.State) || previousChildIsModified);

        if (node.State is AggregateNodeState.Unchanged && childrenIsModified)
        {
            node.State = AggregateNodeState.Modified;
        }

        return node.State is not AggregateNodeState.Unchanged;
    }

    private static async Task<AggregateNode> AddAggregateChain(
        this Dictionary<EntityEntry, AggregateNode> nodeByEntry,
        EntityEntry entry,
        CancellationToken cancellationToken)
    {
        if (nodeByEntry.TryGetValue(entry, out var node))
        {
            // We have already added this entry, so just return the node without expanding its parents or children.
            return node;
        }

        nodeByEntry[entry] = node = entry.ToAggregateNode();
        await nodeByEntry.AddAggregateParentChain(entry, cancellationToken);
        await nodeByEntry.AddAggregateChildChain(entry, cancellationToken);
        return node;
    }

    private static async Task AddAggregateParentChain(
        this Dictionary<EntityEntry, AggregateNode> nodeByEntry,
        EntityEntry childEntry,
        CancellationToken cancellationToken)
    {
        if (!nodeByEntry.TryGetValue(childEntry, out var childNode))
        {
            throw new UnreachableException();
        }

        foreach (var parentForeignKey in childEntry.Metadata.FindAggregateParents())
        {
            // Supports only dependent to principal. That is - one-to-one and one-to-many
            // relationships. Many-to-many relationships is not supported.
            var parentType = parentForeignKey.PrincipalEntityType.ClrType;

            var parentPrimaryKey = parentForeignKey
                .Properties
                .Select(key => childEntry.OriginalValues[key.Name])
                .ToArray();

            if (parentPrimaryKey.Length == 0 || parentPrimaryKey.Any(x => x is null))
            {
                throw new UnreachableException(
                    $"Foreign key to {parentType.Name} from {childEntry.Metadata.ClrType.Name} " +
                    $"is empty or contains null values.");
            }

            var parentEntity = await childEntry.Context
                .FindAsync(parentType, parentPrimaryKey, cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException(
                    $"Could not find parent {parentType.Name} on {childEntry.Metadata.ClrType.Name} " +
                    $"with key [{string.Join(",", parentPrimaryKey)}].");

            var parentEntry = childEntry.Context.Entry(parentEntity);
            var parentNode = await nodeByEntry.AddAggregateChain(parentEntry, cancellationToken);
            parentNode.AddChild(childNode);
            childNode.AddParent(parentNode);
        }
    }

    private static async Task AddAggregateChildChain(
        this Dictionary<EntityEntry, AggregateNode> nodeByEntry,
        EntityEntry parentEntry,
        CancellationToken cancellationToken)
    {
        if (!nodeByEntry.TryGetValue(parentEntry, out var parentNode))
        {
            throw new UnreachableException();
        }

        foreach (var childForeignKey in parentEntry.Metadata.FindAggregateChildren())
        {
            var childNav = parentEntry.Navigation(childForeignKey.PrincipalToDependent!.Name);
            if (!childNav.IsLoaded)
            {
                // TODO: Log warning? 
            }

            // TODO: Fungerer dette for slettede elementer? Burde det vøre OriginalValue i stede for CurrentValue?
            var originalValues = parentEntry.OriginalValues[childForeignKey.PrincipalToDependent!.Name];
            var currentValues = childForeignKey.PrincipalToDependent!.IsCollection
                ? childNav.CurrentValue as IEnumerable<object> ?? Enumerable.Empty<object>()
                : Enumerable.Empty<object>()
                    .Append(childNav.CurrentValue)
                    .Where(x => x is not null)
                    .Cast<object>();

            foreach (var childEntry in currentValues.Select(parentEntry.Context.Entry))
            {
                var childNode = await nodeByEntry.AddAggregateChain(childEntry, cancellationToken);
                parentNode.AddChild(childNode);
                childNode.AddParent(parentNode);
            }
        }
    }

    private static AggregateNode ToAggregateNode(this EntityEntry entry)
    {
        var aggregateState = entry.State switch
        {
            EntityState.Detached => AggregateNodeState.Unchanged,
            EntityState.Unchanged => AggregateNodeState.Unchanged,
            EntityState.Deleted => AggregateNodeState.Deleted,
            EntityState.Modified => AggregateNodeState.Modified,
            EntityState.Added => AggregateNodeState.Added,
            _ => throw new UnreachableException(),
        };

        // Add this to modifiedProperties if needed in future.
        //var modifiedProperties = entry.Properties
        //.Where(x => x.IsModified && !x.IsTemporary)
        //.Select(x => AggregateNodeProperty.Create(
        //    x.Metadata.ClrType,
        //    x.Metadata.Name,
        //    x.OriginalValue,
        //    x.CurrentValue));

        return AggregateNode.Create(
            entry.Entity.GetType(),
            entry.Entity,
            aggregateState,
            Enumerable.Empty<AggregateNodeProperty>());
    }

    private static IEnumerable<IReadOnlyForeignKey> FindAggregateParents(this IReadOnlyEntityType entityType)
    {
        return entityType
            .GetForeignKeys()
            .Where(x => x
                .PrincipalToDependent?
                .PropertyInfo?
                .GetCustomAttribute<AggregateChildAttribute>()
                is not null);
    }

    private static IEnumerable<IReadOnlyForeignKey> FindAggregateChildren(this IReadOnlyEntityType entry)
    {
        return entry
            .GetReferencingForeignKeys()
            .Where(x => x
                .PrincipalToDependent?
                .PropertyInfo?
                .GetCustomAttribute<AggregateChildAttribute>()
                is not null);
    }

    private sealed class EntityEntryComparer : IEqualityComparer<EntityEntry>
    {
        public bool Equals(EntityEntry? x, EntityEntry? y) => ReferenceEquals(x?.Entity, y?.Entity);
        public int GetHashCode(EntityEntry obj) => obj.Entity.GetHashCode();
    }
}
