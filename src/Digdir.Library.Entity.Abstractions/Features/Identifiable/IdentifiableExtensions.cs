namespace Digdir.Library.Entity.Abstractions.Features.Identifiable;

/// <summary>
/// Provides extension methods for <see cref="IIdentifiableEntity"/>.
/// </summary>
public static class IdentifiableExtensions
{
    /// <summary>
    /// Populates <see cref="IIdentifiableEntity.Id"/> with a new guid if the id is <see cref="Guid.Empty"/>.
    /// </summary>
    /// <param name="identifiable">The <see cref="IIdentifiableEntity"/> to update.</param>
    public static Guid CreateId(this IIdentifiableEntity identifiable)
    {
        return identifiable.Id = identifiable.Id == Guid.Empty
            ? Guid.NewGuid()
            : identifiable.Id;
    }
}
