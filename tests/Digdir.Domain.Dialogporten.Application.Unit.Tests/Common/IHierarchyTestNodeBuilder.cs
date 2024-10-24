namespace Digdir.Domain.Dialogporten.Application.Unit.Tests.Common;

internal interface IHierarchyTestNodeBuilder
{
    IHierarchyTestNodeBuilder CreateNewCyclicHierarchy(int depth, params Guid[] ids);
    IHierarchyTestNodeBuilder CreateNewHierarchy(int depth, params Guid[] ids);
    IHierarchyTestNodeBuilder FromNode(Guid id);
    IHierarchyTestNodeBuilder AddWidth(int width, params Guid[] ids);
    IHierarchyTestNodeBuilder AddDepth(int depth, params Guid[] ids);
    IReadOnlyCollection<HierarchyTestNode> Build();
}

internal sealed class HierarchyTestNodeBuilder : IHierarchyTestNodeBuilder
{
    private readonly Dictionary<Guid, HierarchyTestNode> _nodes = [];
    private HierarchyTestNode _current;

    private HierarchyTestNodeBuilder(IEnumerable<HierarchyTestNode> nodes)
    {
        _current = AddRangeReturnFirst(nodes);
    }

    public static IHierarchyTestNodeBuilder CreateNewHierarchy(int depth, params Guid[] ids)
    {
        return new HierarchyTestNodeBuilder(HierarchyTestNode.CreateDepth(depth, ids: ids));
    }

    public static IHierarchyTestNodeBuilder CreateNewCyclicHierarchy(int depth, params Guid[] ids)
    {
        return new HierarchyTestNodeBuilder(HierarchyTestNode.CreateCyclicDepth(depth, ids));
    }

    IHierarchyTestNodeBuilder IHierarchyTestNodeBuilder.CreateNewHierarchy(int depth, params Guid[] ids)
    {
        _current = AddRangeReturnFirst(HierarchyTestNode.CreateDepth(depth, ids: ids));
        return this;
    }

    IHierarchyTestNodeBuilder IHierarchyTestNodeBuilder.CreateNewCyclicHierarchy(int depth, params Guid[] ids)
    {
        _current = AddRangeReturnFirst(HierarchyTestNode.CreateCyclicDepth(depth, ids));
        return this;
    }

    IHierarchyTestNodeBuilder IHierarchyTestNodeBuilder.FromNode(Guid id)
    {
        _current = _nodes[id];
        return this;
    }

    IHierarchyTestNodeBuilder IHierarchyTestNodeBuilder.AddWidth(int width, params Guid[] ids)
    {
        if (width == 0) return this;
        AddRangeReturnFirst(_current.CreateChildrenWidth(width, ids));
        return this;
    }

    IHierarchyTestNodeBuilder IHierarchyTestNodeBuilder.AddDepth(int depth, params Guid[] ids)
    {
        AddRangeReturnFirst(_current.CreateChildrenDepth(depth, ids));
        return this;
    }

    IReadOnlyCollection<HierarchyTestNode> IHierarchyTestNodeBuilder.Build() => _nodes.Values;

    private HierarchyTestNode AddRangeReturnFirst(IEnumerable<HierarchyTestNode> nodes)
    {
        using var nodeEnumerator = nodes.GetEnumerator();
        if (!nodeEnumerator.MoveNext())
        {
            throw new InvalidOperationException("Expected at least one node.");
        }

        var first = nodeEnumerator.Current;
        _nodes.Add(first.Id, first);
        while (nodeEnumerator.MoveNext())
        {
            _nodes.Add(nodeEnumerator.Current.Id, nodeEnumerator.Current);
        }

        return first;
    }
}
