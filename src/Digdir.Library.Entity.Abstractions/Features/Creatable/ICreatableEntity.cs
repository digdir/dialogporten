namespace Digdir.Library.Entity.Abstractions.Features.Creatable;

/// <summary>
/// Abstraction implemented by entities that can be created during the application lifetime.
/// </summary>
public interface ICreatableEntity
{
    /// <summary>
    /// Time at which the entity was first created in UTC.
    /// </summary>
    DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// Id of the user that created the entity. 
    /// </summary>
    Guid CreatedByUserId { get; set; }
}
