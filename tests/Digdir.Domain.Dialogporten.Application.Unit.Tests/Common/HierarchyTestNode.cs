namespace Digdir.Domain.Dialogporten.Application.Unit.Tests.Common;

internal sealed class HierarchyTestNode
{
    private readonly List<HierarchyTestNode> _children = [];

    public Guid Id { get; }
    public Guid? ParentId => Parent?.Id;
    public HierarchyTestNode? Parent { get; private set; }
    public IReadOnlyCollection<HierarchyTestNode> Children => _children;

    private HierarchyTestNode(Guid? id = null, HierarchyTestNode? parent = null)
    {
        Id = id.HasValue && id.Value != default ? id.Value : Guid.NewGuid();
        Parent = parent;
    }

    public IEnumerable<HierarchyTestNode> CreateChildrenWidth(int width, params Guid[] ids) => CreateWidth(width, this, ids);

    public IEnumerable<HierarchyTestNode> CreateChildrenDepth(int depth, params Guid[] ids) => CreateDepth(depth, this, ids);

    public static HierarchyTestNode Create(Guid? id = null, HierarchyTestNode? parent = null)
    {
        var node = new HierarchyTestNode(id, parent);
        parent?._children.Add(node);
        return node;
    }

    public static IEnumerable<HierarchyTestNode> CreateDepth(int depth, HierarchyTestNode? from = null, params Guid[] ids)
    {
        for (var i = 0; i < depth; i++)
        {
            yield return from = Create(ids.ElementAtOrDefault(i), from);
        }
    }

    public static IEnumerable<HierarchyTestNode> CreateWidth(int width, HierarchyTestNode from, params Guid[] ids)
    {
        for (var i = 0; i < width; i++)
        {
            yield return Create(ids.ElementAtOrDefault(i), from);
        }
    }

    public static IEnumerable<HierarchyTestNode> CreateCyclicDepth(int depth, params Guid[] ids)
    {
        if (depth < 1)
        {
            yield break;
        }

        var last = Create(ids.ElementAtOrDefault(depth - 1));
        var current = last;
        foreach (var element in CreateDepth(depth - 1, from: current, ids))
        {
            yield return current = element;
        }

        last.Parent = current;
        current._children.Add(last);
        yield return last;
    }
}
