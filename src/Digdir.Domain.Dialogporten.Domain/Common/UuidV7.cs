namespace Digdir.Domain.Dialogporten.Domain.Common;

public static class UuidV7
{
    private const int Version = 7;
    private const int Variant = 2;
    private const long UnixEpochMilliseconds = 62_135_596_800_000;
    private const long TicksPerMillisecond = 10_000;

    public static bool TryParse(ReadOnlySpan<char> value, out Guid result)
        => Guid.TryParse(value, out result) && IsValid(result);

    public static bool IsValid(Guid value) => IsValid(value.ToByteArray());

    public static bool TryToDateTimeOffset(Guid value, out DateTimeOffset result)
        => TryToDateTimeOffset(value.ToByteArray(), out result);

    private static bool TryToDateTimeOffset(ReadOnlySpan<byte> bytes, out DateTimeOffset result)
    {
        // The timestamp is stored in the first 6 bytes, in little endian order.
        var unixMs = ((long)bytes[3] << 40)
                     | ((long)bytes[2] << 32)
                     | ((long)bytes[1] << 24)
                     | ((long)bytes[0] << 16)
                     | ((long)bytes[5] << 8)
                     | bytes[4];

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
        // The version is stored in the 7th byte, but the nibbles are reversed, so the version is actually in the higher 4 bits.
        var version = bytes[7] >> 4;

        // The variant is stored in the 9th byte, in the higher bits.
        var variant = bytes[8] >> 6;

        return version == Version && variant == Variant; // The variant for UUIDv7 should be '10' in binary
    }
}
