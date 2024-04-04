using System.Security.Cryptography;
using System.Text;

namespace Digdir.Domain.Dialogporten.Application.Common;

internal static class MappingUtils
{
    internal static byte[] GetHashSalt(int size = 16) => RandomNumberGenerator.GetBytes(size);

    internal static string? HashPid(string? personIdentifier, byte[] salt)
    {
        if (string.IsNullOrWhiteSpace(personIdentifier))
        {
            return null;
        }

        var identifierBytes = Encoding.UTF8.GetBytes(personIdentifier);
        Span<byte> buffer = stackalloc byte[identifierBytes.Length + salt.Length];
        identifierBytes.CopyTo(buffer);
        salt.CopyTo(buffer[identifierBytes.Length..]);

        var hashBytes = SHA256.HashData(buffer);

        return BitConverter
            .ToString(hashBytes, 0, 5)
            .Replace("-", "")
            .ToLowerInvariant();
    }
}
