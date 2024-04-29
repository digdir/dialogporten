using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace Digdir.Domain.Dialogporten.Application.Common;

internal interface IStringHasher
{
    [return: NotNullIfNotNull(nameof(personIdentifier))]
    string? Hash(string? personIdentifier);
}

internal class PersistentRandomSaltStringHasher : IStringHasher
{
    public const int StringLength = 10;
    private const int SaltSize = 16;
    private readonly Lazy<byte[]> _lazySalt = new(() => RandomNumberGenerator.GetBytes(SaltSize));

    public string? Hash(string? personIdentifier)
    {
        if (string.IsNullOrWhiteSpace(personIdentifier))
        {
            return null;
        }

        var identifierBytes = Encoding.UTF8.GetBytes(personIdentifier);
        Span<byte> buffer = stackalloc byte[identifierBytes.Length + _lazySalt.Value.Length];
        identifierBytes.CopyTo(buffer);
        _lazySalt.Value.CopyTo(buffer[identifierBytes.Length..]);

        var hashBytes = SHA256.HashData(buffer);

        return BitConverter.ToString(hashBytes, 0, StringLength / 2).Replace("-", "").ToLowerInvariant();
    }
}
