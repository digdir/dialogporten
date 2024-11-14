namespace Digdir.Library.Entity.Abstractions.Features.SoftDeletable;

/// <summary>
/// Abstraction implemented by entities that can be soft-deleted during the application lifetime.
/// </summary>
public interface ISoftDeletableEntity
{
    /// <summary>
    /// Indicating whether the entity is soft-deleted.
    /// True if the entity is soft-deleted, otherwise false.
    /// </summary>
    bool Deleted { get; set; }

    /// <summary>
    /// Time at which the entity was soft-deleted in UTC.
    /// </summary>
    /// <remarks>
    /// The value will be null if the entity has never been soft-deleted, or it is restored.
    /// </remarks>
    DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// Marks an entity as soft deleted.
    /// </summary>
    void SoftDelete() => Deleted = true;
}
