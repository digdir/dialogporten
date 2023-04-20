namespace Digdir.Library.Entity.Abstractions.Features.Identifiable;

/// <summary>
/// Abstraction implemented by entities that can be identified.
/// </summary>
public interface IIdentifiableEntity
{
    /// <summary>
    /// The internal entity identification used primaraly for better clustered index.
    /// </summary>
    public long InternalId { get; set; }

    /// <summary>
    /// The entity identification.
    /// </summary>
    public Guid Id { get; set; }
}
