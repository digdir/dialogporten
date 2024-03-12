namespace Digdir.Domain.Dialogporten.Domain.Common;

public static class UuidV7
{
    public static bool IsValid(string uuid)
    {
        if (!Guid.TryParse(uuid, out var guid))
        {
            return false;
        }

        return IsValid(guid);
    }

    public static bool IsValid(Guid value)
    {
        var bytes = value.ToByteArray();
        // Version is stored in the 7th byte, but the nibbles are reversed so version is actually in the higher 4 bits
        var version = bytes[7] >> 4;
        if (version != 7) // Ensure version is 7
        {
            return false;
        }
        // Variant is stored in the 8th byte, in the higher bits
        var variant = bytes[8] >> 6;
        if (variant != 2) // The variant for UUIDv7 should be '10' in binary
        {
            return false;
        }
        return true;
    }
}
