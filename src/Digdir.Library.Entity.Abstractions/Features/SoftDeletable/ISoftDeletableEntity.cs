namespace Digdir.Library.Entity.Abstractions.Features.SoftDeletable;

/// <summary>
/// Abstraction implemented by entities that can be soft deleted during the application lifetime.
/// </summary>
public interface ISoftDeletableEntity
{
    /// <summary>
    /// Indicating wether or not the entity is soft deleted. 
    /// True if the entity is soft deleted, otherwise false.
    /// </summary>
    bool Deleted { get; set; }

    /// <summary>
    /// Time at which the entity was soft deleted in UTC.
    /// </summary>
    /// <remarks>
    /// Value will be null if the entity never has been soft deleted, or it is restored.
    /// </remarks>
    DateTime? DeletedAtUtc { get; set; }

    /// <summary>
    /// Id of the user that deleted the entity.
    /// </summary>
    /// /// <remarks>
    /// Value will be null if the entity never has been soft deleted, or it is restored.
    /// </remarks>
    Guid? DeletedByUserId { get; set; }
}
