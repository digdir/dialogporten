using Medo;

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
        // We want Guids in big endian format. The default behavior of Medo is big endian,
        // however, the implicit conversion from Medo.Uuid7 to Guid is little endian.
        // "matchGuidEndianness" is set to true to ensure big endian.
        return identifiable.Id = identifiable.Id == Guid.Empty
            ? Uuid7.NewUuid7().ToGuid(matchGuidEndianness: true)
            : identifiable.Id;
    }
}
