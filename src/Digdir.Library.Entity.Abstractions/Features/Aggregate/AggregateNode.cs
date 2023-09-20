namespace Digdir.Library.Entity.Abstractions.Features.Aggregate;

public abstract class AggregateNode
{
    private static readonly Type _openGenericAggregateNodeType = typeof(AggregateNode<>);

    private readonly List<AggregateNode> _children = new();
    public object Entity { get; private set; }
    public IReadOnlyCollection<AggregateNode> Children => _children;

    protected AggregateNode() { }

    internal void AddChild(AggregateNode node) => _children.Add(node);

    internal static AggregateNode Create(object entity) => Create(entity.GetType(), entity);

    private static AggregateNode Create(Type type, object entity)
    {
        if (!entity.GetType().IsAssignableTo(type))
        {
            throw new ArgumentException(
                $"Parameter {nameof(entity)} ({entity.GetType()}) must be assignable to {type}.");
        }

        var node = (AggregateNode)Activator.CreateInstance(_openGenericAggregateNodeType.MakeGenericType(type))!;
        node.Entity = entity;
        return node;
    }

    public IEnumerable<AggregateNode<T>> GetChildrenOf<T>()
    {
        return Children
            .Where(x => x is AggregateNode<T>)
            .Cast<AggregateNode<T>>();
    }
}

public sealed class AggregateNode<T> : AggregateNode
{
    public new T Entity => (T)base.Entity;
    public AggregateNode() : base() { }
    internal static AggregateNode<T> Create(T entity) => (AggregateNode<T>)AggregateNode.Create(entity);
}