using System.Security.Cryptography;
using Digdir.Domain.Dialogporten.Domain.Common;
using Digdir.Library.Entity.Abstractions.Features.Identifiable;

namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;

public static class DeterministicUuidV7
{
    public static Guid Generate(DateTimeOffset timestamp, Guid seed)
    {
        // Generate base UUIDv7 structure
        var uuid = IdentifiableExtensions.CreateVersion7(timestamp);
        var uuidBytes = uuid.ToByteArray();

        // Convert timestamp to milliseconds since Unix epoch
        var milliseconds = timestamp.ToUnixTimeMilliseconds();
        var timestampBytes = BitConverter.GetBytes(milliseconds);

        // Ensure timestamp bytes are little-endian
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(timestampBytes);
        }

        // Prepare seed and timestamp combination
        var seedBytes = seed.ToByteArray();
        var combinedBytes = new byte[seedBytes.Length + timestampBytes.Length];
        Buffer.BlockCopy(seedBytes, 0, combinedBytes, 0, seedBytes.Length);
        Buffer.BlockCopy(timestampBytes, 0, combinedBytes, seedBytes.Length, timestampBytes.Length);

        // Generate hash
        var hashBytes = SHA256.HashData(combinedBytes);

        // Replace random bytes in the UUID
        for (var i = 10; i < 16; i++)
        {
            uuidBytes[i] = hashBytes[i];
        }

        // Set version to 7 (UUIDv7)
        uuidBytes[6] = (byte)((uuidBytes[6] & 0x0F) | 0x70);

        // Set variant to RFC 4122 (10xx xxxx)
        uuidBytes[8] = (byte)((uuidBytes[8] & 0x3F) | 0x80);

        var deterministicUuid = new Guid(uuidBytes);

        // Debugging output
        Console.WriteLine($"Generated UUID: {deterministicUuid}");
        Console.WriteLine($"CombinedBytes: {BitConverter.ToString(combinedBytes)}");

        return deterministicUuid;
    }

}
