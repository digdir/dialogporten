namespace Digdir.Library.Entity.Abstractions.Features.Aggregate;

/// <summary>
/// Used to define a relationship from parent to child in the aggregate tree
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class AggregateChildAttribute : Attribute;
