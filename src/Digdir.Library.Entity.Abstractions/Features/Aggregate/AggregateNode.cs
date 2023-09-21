namespace Digdir.Library.Entity.Abstractions.Features.Aggregate;

public abstract class AggregateNode
{
    private static readonly Type _openGenericAggregateNodeType = typeof(AggregateNode<>);

    private readonly List<AggregateNode> _children = new();
    public object Entity { get; private set; }
    public IReadOnlyCollection<AggregateNode> Children => _children;

    protected AggregateNode() { }

    internal void AddChild(AggregateNode node) => _children.Add(node);

    internal static AggregateNode<T> Create<T>(T entity) => 
        (AggregateNode<T>)Create(typeof(T), entity ?? throw new ArgumentNullException(nameof(entity)));
    internal static AggregateNode Create(object entity) => 
        Create(entity.GetType(), entity);
    internal static AggregateNode Create(Type type, object entity)
    {
        if (!entity.GetType().IsAssignableTo(type))
        {
            throw new ArgumentException(
                $"Parameter {nameof(entity)} ({entity.GetType()}) must be assignable to {type}.");
        }

        var genericType = _openGenericAggregateNodeType.MakeGenericType(type);
        var node = (AggregateNode) Activator.CreateInstance(genericType, nonPublic: true)!;
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
    public new T Entity => (T) base.Entity;
    private AggregateNode() : base() { }
}
