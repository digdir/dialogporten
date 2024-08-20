namespace Digdir.Domain.Dialogporten.Domain.Common;

public static class UuidV7
{
    private const int Version = 7;
    private const int Variant = 2;
    private const long UnixEpochMilliseconds = 62_135_596_800_000;
    private const long TicksPerMillisecond = 10_000;

    public static bool TryParse(ReadOnlySpan<char> value, out Guid result)
        => Guid.TryParse(value, out result) && IsValid(result);

    public static bool IsValid(Guid value) => IsValid(ToBigEndianByteArray(value));

    public static bool TryToDateTimeOffset(Guid value, out DateTimeOffset result)
    {
        var bytes = ToBigEndianByteArray(value);
        return TryToDateTimeOffset(bytes, out result);
    }

    /// <summary>
    /// Converts a Guid to a big endian byte array. We expect big endian Guids throughout the system.
    /// However, the default behavior of Guids is to treat them as little endian.
    /// This method is to make it painfully obvious to the reader.
    /// </summary>
    /// <param name="value">The Guid to convert</param>
    /// <returns></returns>
    private static ReadOnlySpan<byte> ToBigEndianByteArray(Guid value) => value.ToByteArray(bigEndian: true);

    private static bool TryToDateTimeOffset(ReadOnlySpan<byte> bytes, out DateTimeOffset result)
    {
        // The timestamp is stored in the first 6 bytes, in big endian order.
        var unixMs = ((long)bytes[0] << 40)
                     | ((long)bytes[1] << 32)
                     | ((long)bytes[2] << 24)
                     | ((long)bytes[3] << 16)
                     | ((long)bytes[4] << 8)
                     | bytes[5];

        var ticks = (UnixEpochMilliseconds + unixMs) * TicksPerMillisecond;

        if (ticks < DateTimeOffset.MinValue.Ticks || ticks > DateTimeOffset.MaxValue.Ticks)
        {
            result = default;
            return false;
        }

        result = new DateTimeOffset(ticks, TimeSpan.Zero);
        return true;
    }

    private static bool IsValid(ReadOnlySpan<byte> bytes)
    {
        // The version is stored in the 7th byte, but the nibbles are reversed so version is actually in the higher 4 bits.
        var version = bytes[6] >> 4;

        // The variant is stored in the 9th byte, in the higher bits.
        var variant = bytes[8] >> 6;

        return version == Version && variant == Variant; // The variant for UUIDv7 should be '10' in binary
    }
}
