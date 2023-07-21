namespace Digdir.Library.Entity.Abstractions.Features.Updatable;

/// <summary>
/// Abstraction implemented by entities that can be updated during the application lifetime.
/// </summary>
public interface IUpdateableEntity : IEntityBase
{
    /// <summary>
    /// Time at which the entity was last updated in UTC.
    /// </summary>
    DateTimeOffset UpdatedAt { get; set; }
}
