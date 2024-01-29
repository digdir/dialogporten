using System.Security.Cryptography;

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

        const int keySize = 32;
        const int iterations = 5_000;
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            personIdentifier,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            keySize
        );

        return Convert.ToHexString(hash)[..5];
    }
}
