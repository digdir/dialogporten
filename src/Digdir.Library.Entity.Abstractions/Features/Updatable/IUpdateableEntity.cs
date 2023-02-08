namespace Digdir.Library.Entity.Abstractions.Features.Updatable;

/// <summary>
/// Abstraction implemented by entities that can be updated during the application lifetime.
/// </summary>
public interface IUpdateableEntity
{
    /// <summary>
    /// Time at which the entity was last updated in UTC.
    /// </summary>
    DateTime UpdatedAtUtc { get; set; }

    /// <summary>
    /// Id of the user that last updated the entity.
    /// </summary>
    Guid UpdatedByUserId { get; set; }
}
