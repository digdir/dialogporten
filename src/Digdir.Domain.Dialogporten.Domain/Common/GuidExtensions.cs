using Medo;

namespace Digdir.Domain.Dialogporten.Domain.Common;

public static class GuidExtensions
{
    public static Guid GenerateBigEndianUuidV7IfEmpty(this Guid? value)
    {
        // We want Guids in big endian format. The default behavior of Medo is big endian,
        // however, the implicit conversion from Medo.Uuid7 to Guid is little endian.
        // "matchGuidEndianness" is set to true to ensure big endian.
        return !value.HasValue || value.Value == default
            ? Uuid7.NewUuid7().ToGuid(matchGuidEndianness: true)
            : value.Value;
    }
}
