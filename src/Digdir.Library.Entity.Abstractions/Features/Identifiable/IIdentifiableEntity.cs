namespace Digdir.Library.Entity.Abstractions.Features.Identifiable;

/// <summary>
/// Abstraction implemented by entities that can be identified.
/// </summary>
public interface IIdentifiableEntity
{
    /// <summary>
    /// The entity identification.
    /// </summary>
    public Guid Id { get; set; }
}
