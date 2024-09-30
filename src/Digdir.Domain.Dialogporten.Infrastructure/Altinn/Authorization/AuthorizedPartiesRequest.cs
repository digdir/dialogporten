using System.Security.Cryptography;
using System.Text;
using Digdir.Domain.Dialogporten.Domain.Parties.Abstractions;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.Authorization;

internal sealed class AuthorizedPartiesRequest(IPartyIdentifier partyIdentifier)
{
    public string Type { get; init; } = partyIdentifier.Prefix();
    public string Value { get; init; } = partyIdentifier.Id;
}

internal static class AuthorizedPartiesRequestExtensions
{
    public static string GenerateCacheKey(this AuthorizedPartiesRequest request)
    {
        var rawKey = $"{request.Type}:{request.Value}";

        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawKey));
        var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

        return $"auth:parties:{hashString}";
    }
}
