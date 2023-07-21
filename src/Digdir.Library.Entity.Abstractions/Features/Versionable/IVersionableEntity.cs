namespace Digdir.Library.Entity.Abstractions.Features.Versionable;

/// <summary>
/// Abstraction implemented by entities to keep track of their version.
/// </summary>
public interface IVersionableEntity : IEntityBase
{
    /// <summary>
    /// The entity tag unique to each version of the entity.
    /// </summary>
    public Guid ETag { get; set; }
}
