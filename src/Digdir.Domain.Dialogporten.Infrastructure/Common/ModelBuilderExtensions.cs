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
        // var tree =
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

        // TreeNode? root = null;
        //
        // foreach (var (parent, children) in treeDict)
        // {
        //     if (root is null)
        //     {
        //         root = new TreeNode(parent);
        //         root.Children.AddRange(children.Select(x => new TreeNode(x)));
        //         foreach (var child in children)
        //         {
        //             var  = treeDict[child];
        //             child
        //         }
        //     }
        //     
        //     
        // }
        // convert to tree 
        // return tree;
    }
    
    private static List<TreeNode> GetAggregateRoots(this IDictionary<EntityEntry, HashSet<EntityEntry>> treeDict, HashSet<EntityEntry>? potentialRoots = null)
    {
        potentialRoots ??= new HashSet<EntityEntry>(comparer: new EntityEntryComparer());

        foreach (var (entry, children) in treeDict)
        {
            // Hvis vi finner en potential root som er en child
            // Så er den ikke en root
            // Hiv den ut og sett inn den bedre potensielle roten med den forige "roten" som child.
            // Trenger ikke traversere child rooten for den er alt traversert.
            // HashSet<TreeNode> må vite hvordan de er like
            if (!potentialRoots.Add(entry))
            {
                continue;
            }

            var node = new TreeNode 
            { 
                Data = entry, 
                Children = GetAggregateChildren(children, treeDict, potentialRoots)
            };

            // TODO: Legg til noder? 
        }
    }

    private static List<TreeNode> GetAggregateChildren(
        HashSet<EntityEntry> entityEntries, 
        IDictionary<EntityEntry, HashSet<EntityEntry>> treeDict, 
        HashSet<EntityEntry> visited)
    {
        var result = new List<TreeNode>();

        foreach (var entityEntry in entityEntries)
        {
            if (!visited.Add(entityEntry))
            {
                continue;
            }

            if (!treeDict.TryGetValue(entityEntry, out var children))
            {
                throw new Exception();
            }

            var node = new TreeNode
            {
                Data = entityEntry,
                Children = GetAggregateChildren(children, treeDict, visited)
            };

            result.Add(node);
        }

        return result;
    }

    private static async Task GetOwnershipChainInclusive(
        this EntityEntry entry,
        IDictionary<EntityEntry, HashSet<EntityEntry>> treeDict)
    {
        if (!treeDict.ContainsKey(entry))
        {
            treeDict[entry] = new HashSet<EntityEntry>(comparer: new EntityEntryComparer());
        }
        
        foreach (var parentForeignKey in entry.Metadata.FindAggregateParents())
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
    public required object Data { get; init; }
    public List<TreeNode> Children { get; init; } = new();

    internal void AddChild(TreeNode treeNode)
    {
        Children.Add(treeNode);
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