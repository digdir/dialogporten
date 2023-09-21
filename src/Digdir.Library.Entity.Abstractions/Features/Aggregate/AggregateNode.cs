namespace Digdir.Library.Entity.Abstractions.Features.Aggregate;

/// <summary>
/// Represents a node in an aggregate tree. 
/// </summary>
public abstract class AggregateNode
{
    private static readonly Type _openGenericAggregateNodeType = typeof(AggregateNode<>);
    private readonly List<AggregateNode> _children = new();

    /// <summary>
    /// The actual entity in the aggregate tree this node represents.
    /// </summary>
    public object Entity { get; private set; } = null!;

    /// <summary>
    /// A collection of modified children. A child node is modified if it itself 
    /// is modified, or one of its children are modified. Modified in this 
    /// context meens added, modified, or deleted. 
    /// </summary>
    public IReadOnlyCollection<AggregateNode> Children => _children;

    /// <summary>
    /// The state of the <see cref="Entity"/> this node represents.
    /// </summary>
    public AggregateNodeState State { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateNode"/> class.
    /// </summary>
    protected AggregateNode() { }

    internal void AddChild(AggregateNode node) => _children.Add(node);

    internal static AggregateNode<T> Create<T>(T entity, AggregateNodeState state) => 
        (AggregateNode<T>)Create(typeof(T), entity ?? throw new ArgumentNullException(nameof(entity)), state);

    internal static AggregateNode Create(object entity, AggregateNodeState state) => 
        Create(entity.GetType(), entity, state);

    internal static AggregateNode Create(Type type, object entity, AggregateNodeState state)
    {
        if (!entity.GetType().IsAssignableTo(type))
        {
            throw new ArgumentException(
                $"Parameter {nameof(entity)} ({entity.GetType()}) must be assignable to {type}.");
        }

        var genericType = _openGenericAggregateNodeType.MakeGenericType(type);
        var node = (AggregateNode) Activator.CreateInstance(genericType, nonPublic: true)!;
        node.Entity = entity;
        node.State = state;
        return node;
    }
}

/// <summary>
/// Represents a type specific node in an aggregate tree.
/// </summary>
/// <typeparam name="T">The type this node represents.</typeparam>
public sealed class AggregateNode<T> : AggregateNode
{
    /// <summary>
    /// The actual entity in the aggregate tree this node represents.
    /// </summary>
    public new T Entity => (T) base.Entity;
    private AggregateNode() : base() { }
}

/// <summary>
/// The entity state in which a <see cref="AggregateNode"/> represents.
/// </summary>
public enum AggregateNodeState
{
    /// <summary>
    /// The entity has been added, but does not yet exist in the database.
    /// </summary>
    Added = 1,

    /// <summary>
    /// All or some of the entities property values have been modified.
    /// </summary>
    Modified = 2,

    /// <summary>
    /// The entity has been marked for deletion from the database.
    /// </summary>
    Deleted = 3,

    /// <summary>
    /// The entitys property values have not been changed from the values in the database.
    /// </summary>
    Unchanged = 4
}