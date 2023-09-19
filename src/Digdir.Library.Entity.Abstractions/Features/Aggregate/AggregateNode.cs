namespace Digdir.Library.Entity.Abstractions.Features.Aggregate;

public sealed class AggregateNode
{
    private readonly List<AggregateNode> _children = new();
    public object Entity { get; }
    public IReadOnlyCollection<AggregateNode> Children => _children;

    public AggregateNode(object entity)
    {
        Entity = entity;
    }

    public void AddChild(AggregateNode node) => _children.Add(node);
}