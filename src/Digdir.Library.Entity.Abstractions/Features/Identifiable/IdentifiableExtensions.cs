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
        => identifiable.Id = CreateVersion7IfDefault(identifiable.Id);

    /// <summary>
    /// Creates a new version 7 UUID if the value is null or <see cref="Guid.Empty"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Guid CreateVersion7IfDefault(this Guid? value)
    {
        // We want Guids in big endian format. The default behavior of Medo is big endian,
        // however, the implicit conversion from Medo.Uuid7 to Guid is little endian.
        // "matchGuidEndianness" is set to true to ensure big endian.
        return !value.HasValue || value.Value == default
            ? Uuid7.NewUuid7().ToGuid(matchGuidEndianness: true)
            : value.Value;
    }
}
