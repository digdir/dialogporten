namespace Digdir.Library.Entity.Abstractions.Features.Aggregate;

/// <summary>
/// Used to define a relationship from child to parent in the aggregate tree
/// <remarks>Only use this tag on dependent side of relationships</remarks>
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class AggregateParentAttribute : Attribute{ }