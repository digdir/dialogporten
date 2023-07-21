namespace Digdir.Library.Entity.Abstractions.Features.Creatable;

/// <summary>
/// Abstraction implemented by entities that can be created during the application lifetime.
/// </summary>
public interface ICreatableEntity : IEntityBase
{
    /// <summary>
    /// Time at which the entity was first created in UTC.
    /// </summary>
    DateTimeOffset CreatedAt { get; set; }
}
