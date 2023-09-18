using System.Linq.Expressions;
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
        
        // this will return tree
        await modifiedEntries.GetOwnershipChainInclusive();
        
        // traverse the tree and call nodes that implement the INotifyChange interface
        // and pass them every child in the tree below them
    }
    

    // private static async IAsyncEnumerable<(EntityEntry Entry, EntityEntry? Child)> GetOwnershipChainInclusive(this IEnumerable<EntityEntry> entries)
    private static async Task GetOwnershipChainInclusive(this IEnumerable<EntityEntry> entries)
    {
        var treeDict = new Dictionary<EntityEntry, HashSet<EntityEntry>> (comparer: new EntityEntryComparer());
        
        foreach (var entry in entries) 
            await entry.GetOwnershipChainInclusive(treeDict);
        
        // convert to tree 
        // return tree;
    }

    private static async Task GetOwnershipChainInclusive(
        this EntityEntry entry,
        IDictionary<EntityEntry, HashSet<EntityEntry>> treeDict)
    {
        var parentForeignKeys = entry.Metadata
            .FindAggregateParents()
            .ToList();
        
        if (!parentForeignKeys.Any() && !treeDict.ContainsKey(entry))
        {
            treeDict[entry] = new HashSet<EntityEntry>(comparer: new EntityEntryComparer());
            return;
        }
        
        foreach (var parentForeignKey in parentForeignKeys)
        {
            var parentType = parentForeignKey.PrincipalEntityType.ClrType;
            
            var parentPrimaryKey = parentForeignKey
                .Properties
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

            if (treeDict.TryGetValue(parentEntry, out var children))
            {
                children.Add(entry);
                continue;
            }
            
            treeDict[parentEntry] = new HashSet<EntityEntry>(new [] {entry}, comparer: new EntityEntryComparer());
            await GetOwnershipChainInclusive(parentEntry, treeDict);
        }
    }
}


internal class EntityEntryComparer : IEqualityComparer<EntityEntry>
{
    public bool Equals(EntityEntry? x, EntityEntry? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if(x is null || y is null) return false;
        return x.Entity == y.Entity;
    }

    public int GetHashCode(EntityEntry obj) => obj.Entity.GetHashCode();
}

internal class TreeNode
{
    public object Data { get; set; }
    public List<TreeNode> Children { get; set; }
    
    public TreeNode(object data)
    {
        Data = data;
        Children = new List<TreeNode>();
    }
    
    public void AddChild(TreeNode child)
    {
        Children.Add(child);
    }
}

internal class Tree
{
    public TreeNode Root { get; set; }
    
    public Tree(object rootData)
    {
        Root = new TreeNode(rootData);
    }
}