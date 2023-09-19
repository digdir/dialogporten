using System.Linq.Expressions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Library.Entity.Abstractions.Features.Updatable;
using Digdir.Library.Entity.Abstractions.Features.Versionable;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Digdir.Library.Entity.EntityFrameworkCore.Features.Aggregate;

public static class AggregateExtensions
{
    private static readonly EntityEntryComparer _entityEntryComparer = new();
    private const string AggregateAnnotationName = "DddAggregateParent";

    public static EntityTypeBuilder<TEntity> HasAggregateParent<TEntity, TProperty>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        Expression<Func<TEntity, TProperty>> navigationExpression)
        where TEntity : class
    {
        var navigationProperty = navigationExpression.GetMemberAccess();

        var foreignKey = entityTypeBuilder
            .Metadata
            .FindNavigation(navigationProperty)
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

    internal static async Task HandleAggregateEntities(this ChangeTracker changeTracker,
        DateTimeOffset utcNow, CancellationToken cancellationToken)
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
        var childrenByParent = new Dictionary<EntityEntry, AggregateNode>(comparer: _entityEntryComparer);

        foreach (var entry in entries)
        {
            await childrenByParent.AddAggregateParentChain(entry, cancellationToken);
        }

        return childrenByParent;
    }

    private static async Task AddAggregateParentChain(
        this Dictionary<EntityEntry, AggregateNode> nodeByEntry,
        EntityEntry entry,
        CancellationToken cancellationToken)
    {
        if (!nodeByEntry.ContainsKey(entry))
        {
            nodeByEntry[entry] = new AggregateNode(entry.Entity);
        }

        foreach (var parentForeignKey in entry.Metadata.FindAggregateParents())
        {
            // Supports only dependent to principal. That is - one-to-one and one-to-many
            // relationships. Many-to-many relationships is not supported.
            var parentType = parentForeignKey.PrincipalEntityType.ClrType;

            var parentPrimaryKey = parentForeignKey
                .Properties
                .Select(key => entry.OriginalValues[key.Name])
                .ToArray();

            if (parentPrimaryKey.All(x => x is null))
            {
                continue;
            }

            var parentEntity =
                await entry.Context.FindAsync(parentType, parentPrimaryKey, cancellationToken: cancellationToken);

            if (parentEntity is null)
            {
                continue;
            }

            var parentEntry = entry.Context.Entry(parentEntity);

            // If the parent is known to the dictionary, then we have
            // already traversed its parent chain from a previous
            // child. We need not traverse it again. Add the current
            // entry as a child and continue.
            if (nodeByEntry.TryGetValue(parentEntry, out var parentNode))
            {
                parentNode.AddChild(nodeByEntry[entry]);
                continue;
            }

            // The parent is unknown to the dictionary, so we add it
            // with the current entry as a child and traverse its
            // parent chain.
            nodeByEntry[parentEntry] = new AggregateNode(parentEntry.Entity);
            nodeByEntry[parentEntry].AddChild(nodeByEntry[entry]);
            await nodeByEntry.AddAggregateParentChain(parentEntry, cancellationToken);
        }
    }

    private sealed class EntityEntryComparer : IEqualityComparer<EntityEntry>
    {
        public bool Equals(EntityEntry? x, EntityEntry? y) => ReferenceEquals(x?.Entity, y?.Entity);
        public int GetHashCode(EntityEntry obj) => obj.Entity.GetHashCode();
    }
}