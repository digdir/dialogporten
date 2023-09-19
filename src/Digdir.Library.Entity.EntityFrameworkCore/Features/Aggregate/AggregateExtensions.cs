using System.Linq.Expressions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Library.Entity.Abstractions.Features.Updatable;
using Digdir.Library.Entity.Abstractions.Features.Versionable;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Digdir.Domain.Dialogporten.Infrastructure.Common;

public static class AggregateExtensions
{
    private static readonly EntityEntryComparer _entityEntryComparer = new();
    private const string AggregateAnnotationName = "DddAggregateParent";

    public static EntityTypeBuilder<TEntity> HasAggregateParent<TEntity, TProperty>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        Expression<Func<TEntity, TProperty>> navigationProperty)
        where TEntity : class
    {
        var foreignKey = entityTypeBuilder
            .Metadata
            .FindNavigation(navigationProperty.GetMemberAccess())
            .ForeignKey;
        foreignKey.AddAnnotation(AggregateAnnotationName, null);

        return entityTypeBuilder;
    }

    private static IEnumerable<IForeignKey> FindAggregateParents(this IEntityType entityType)
    {
        return entityType
            .GetForeignKeys()
            .Where(x => x.FindAnnotation(AggregateAnnotationName) is not null);
    }

    internal static async Task HandleAggregateEntities(this ChangeTracker changeTracker, DateTimeOffset utcNow, CancellationToken cancellationToken)
    {
        var aggregateNodeByEntry = await changeTracker
            .Entries()
            .Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .GetAggregateNodeByEntry(cancellationToken);

        foreach (var (entry, aggregateNode) in aggregateNodeByEntry)
        {
            if (entry.Entity is INotifyAggregateCreated created && entry.State is EntityState.Added)
            {
                created.Created(aggregateNode, utcNow);
            }

            if (entry.Entity is INotifyAggregateUpdated updated && 
                entry.State is EntityState.Modified or EntityState.Unchanged)
            {
                updated.Updated(aggregateNode, utcNow);
            }

            if (entry.Entity is INotifyAggregateDeleted deleted && entry.State is EntityState.Deleted)
            {
                deleted.Deleted(aggregateNode, utcNow);
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
    
    private static async Task<IReadOnlyDictionary<EntityEntry, AggregateNode>> GetAggregateNodeByEntry(
        this IEnumerable<EntityEntry> entries, 
        CancellationToken cancellationToken)
    {
        var childrenByParent = new Dictionary<EntityEntry, HashSet<EntityEntry>> (comparer: _entityEntryComparer);
        
        foreach (var entry in entries)
        {
            await childrenByParent.AddAggregateParentChain(entry, cancellationToken);
        }

        return childrenByParent.ToAggregateNodeByEntry();
    }

    private static async Task AddAggregateParentChain2(
        this Dictionary<EntityEntry, AggregateNode> childrenByParent,
        EntityEntry entry,
        CancellationToken cancellationToken)
    {
        if (!childrenByParent.ContainsKey(entry))
        {
            childrenByParent[entry] = new AggregateNode(entry.Entity);
        }

        foreach (var parentForeignKey in entry.Metadata.FindAggregateParents())
        {
            // Supports only dependent to principal. That is - one-to-one and one-to-many
            // relationships. Many-to-many relationships is not supportet.
            var parentType = parentForeignKey.PrincipalEntityType.ClrType;

            var parentPrimaryKey = parentForeignKey
                .Properties
                .Select(key => entry.OriginalValues[key.Name])
                .ToArray();

            if (parentPrimaryKey.All(x => x is null))
            {
                continue;
            }

            var parentEntity = await entry.Context.FindAsync(parentType, parentPrimaryKey, cancellationToken: cancellationToken);

            if (parentEntity is null)
            {
                continue;
            }

            var parentEntry = entry.Context.Entry(parentEntity);

            // If the parent is known to the dictionary, then we have
            // already traversed its parent chain from a previous
            // child. We need not traverse it again. Add the current
            // entry as a child and continue.
            if (childrenByParent.TryGetValue(parentEntry, out var children))
            {
                children.AddChild(new AggregateNode(entry.Entity));
                continue;
            }

            // The parent is unknown to the dictionary, so we add it
            // with the current entry as a child and traverse its
            // parent chain.
            childrenByParent[parentEntry] = new AggregateNode(parentEntry.Entity);
            await childrenByParent.AddAggregateParentChain2(parentEntry, cancellationToken);
        }
    }

    private static async Task AddAggregateParentChain(
        this Dictionary<EntityEntry, HashSet<EntityEntry>> childrenByParent,
        EntityEntry entry,
        CancellationToken cancellationToken)
    {
        if (!childrenByParent.ContainsKey(entry))
        {
            childrenByParent[entry] = new HashSet<EntityEntry>(comparer: _entityEntryComparer);
        }
        
        foreach (var parentForeignKey in entry.Metadata.FindAggregateParents())
        {
            // Supports only dependent to principal. That is - one-to-one and one-to-many
            // relationships. Many-to-many relationships is not supportet.
            var parentType = parentForeignKey.PrincipalEntityType.ClrType;
            
            var parentPrimaryKey = parentForeignKey
                .Properties
                .Select(key => entry.OriginalValues[key.Name])
                .ToArray();

            if (parentPrimaryKey.All(x => x is null))
            {
                continue;
            }

            var parentEntity = await entry.Context.FindAsync(parentType, parentPrimaryKey, cancellationToken: cancellationToken);

            if (parentEntity is null)
            {
                continue;
            }

            var parentEntry = entry.Context.Entry(parentEntity);

            // If the parent is known to the dictionary, then we have
            // already traversed its parent chain from a previous
            // child. We need not traverse it again. Add the current
            // entry as a child and continue.
            if (childrenByParent.TryGetValue(parentEntry, out var children))
            {
                children.Add(entry);
                continue;
            }
            
            // The parent is unknown to the dictionary, so we add it
            // with the current entry as a child and traverse its
            // parent chain.
            childrenByParent[parentEntry] = new HashSet<EntityEntry>(new[]{ entry }, comparer: _entityEntryComparer);
            await childrenByParent.AddAggregateParentChain(parentEntry, cancellationToken);
        }
    }

    private static Dictionary<EntityEntry, AggregateNode> ToAggregateNodeByEntry(
        this Dictionary<EntityEntry, HashSet<EntityEntry>> childrenByParent)
    {
        var nodeByEntry = new Dictionary<EntityEntry, AggregateNode>(comparer: _entityEntryComparer);

        foreach (var (parent, children) in childrenByParent)
        {
            // If the parent is known by the dictionary then its been
            // added and expanded as part of another parent chain.
            if (nodeByEntry.ContainsKey(parent))
            {
                continue;
            }

            nodeByEntry[parent] = new AggregateNode(parent.Entity);
            nodeByEntry[parent].ExpandChildren(children, childrenByParent, nodeByEntry);
        }

        return nodeByEntry;
    }

    private static void ExpandChildren(
        this AggregateNode parentNode,
        IEnumerable<EntityEntry> children,
        Dictionary<EntityEntry, HashSet<EntityEntry>> childrenByParent,
        Dictionary<EntityEntry, AggregateNode> nodeByEntry)
    {
        foreach (var child in children)
        {
            // If the child is not known by the dictionary then we need
            // to create the child node and expand its children.
            if (!nodeByEntry.TryGetValue(child, out var expandedChild))
            {
                nodeByEntry[child] = expandedChild = new AggregateNode(child.Entity);
                nodeByEntry[child].ExpandChildren(childrenByParent[child], childrenByParent, nodeByEntry);
            }

            parentNode.AddChild(expandedChild);
        }
    }

    private sealed class EntityEntryComparer : IEqualityComparer<EntityEntry>
    {
        public bool Equals(EntityEntry? x, EntityEntry? y) => ReferenceEquals(x?.Entity, y?.Entity);
        public int GetHashCode(EntityEntry obj) => obj.Entity.GetHashCode();
    }
}
