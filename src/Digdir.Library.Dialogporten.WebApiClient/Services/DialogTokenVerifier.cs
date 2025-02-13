using System.Buffers.Text;
using System.Collections.ObjectModel;
using System.Text.Json;
using Altinn.ApiClients.Dialogporten.Config;
using Microsoft.Extensions.Options;
using NSec.Cryptography;

namespace Altinn.ApiClients.Dialogporten.Services;

public interface IDialogTokenVerifier
{
    bool Verify(ReadOnlySpan<char> token);
}

internal sealed class DialogTokenVerifier : IDialogTokenVerifier
{
    private readonly string _kid;
    private readonly ReadOnlyCollection<PublicKeyPair> _publicKey;
    public DialogTokenVerifier(IOptions<DialogportenSettings> options)
    {
        _kid = options.Value.Ed25519Keys.Primary.Kid;
        // _publicKey = PublicKey.Import(SignatureAlgorithm.Ed25519,
        // Base64Url.DecodeFromChars(options.Value.Ed25519Keys.Primary.PublicComponent), KeyBlobFormat.RawPublicKey);
        _publicKey = EdDsaSecurityKeysCacheService.PublicKeys;
    }
    public bool Verify(ReadOnlySpan<char> token)
    {
        try // Amund: me no like
        {
            var tokenPartEnumerator = token.Split('.');
            if (!tokenPartEnumerator.MoveNext())
            {
                return false;
            }

            // Amund: Sparer ca 3 operations i IL med å lagre i egen variabel først
            var current = tokenPartEnumerator.Current;
            Span<byte> header = stackalloc byte[current.End.Value - current.Start.Value];
            if (!tokenPartEnumerator.MoveNext() || !Base64Url.TryDecodeFromChars(token[current], header, out var headerLength))
            {
                return false;
            }

            current = tokenPartEnumerator.Current;
            Span<byte> body = stackalloc byte[current.End.Value - current.Start.Value];
            if (!tokenPartEnumerator.MoveNext() || !Base64Url.TryDecodeFromChars(token[current], body, out var bodyLength))
            {
                return false;
            }

            current = tokenPartEnumerator.Current;
            Span<byte> signature = stackalloc byte[current.End.Value - current.Start.Value];
            if (tokenPartEnumerator.MoveNext() && !Base64Url.TryDecodeFromChars(token[current], signature, out _))
            {
                return false;
            }

            var publicKey = EdDsaSecurityKeysCacheService.PublicKeys[0];
            var headerJson = JsonSerializer.Deserialize<JsonElement>(header);
            if (!headerJson.TryGetProperty("kid", out var value) && value.GetString() != publicKey.Kid)
            {
                return false;
            }

            Span<byte> headerAndBody = stackalloc byte[headerLength + bodyLength + 1];
            header[..headerLength].CopyTo(headerAndBody);
            headerAndBody[header.Length] = (byte)'.';
            body[..bodyLength].CopyTo(headerAndBody[(header.Length + 1)..]);

            if (!SignatureAlgorithm.Ed25519.Verify(publicKey.Key, headerAndBody, signature))
            {
                return false;
            }

            var bodyJson = JsonSerializer.Deserialize<JsonElement>(body);

            return TryGetExpires(bodyJson, out var expiresOffset) &&
                   expiresOffset >= DateTimeOffset.UtcNow;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static bool TryGetExpires(JsonElement bodyJson, out DateTimeOffset expires)
    {
        expires = default;
        if (!bodyJson.TryGetProperty(DialogTokenClaimTypes.Expires, out var expiresElement))
        {
            return false;
        }

        if (!expiresElement.TryGetInt32(out var expiresIn))
        {
            return false;
        }
        expires = DateTimeOffset.FromUnixTimeSeconds(expiresIn);
        return true;
    }
}

internal static class DialogTokenClaimTypes
{
    public const string JwtId = "jti";
    public const string Issuer = "iss";
    public const string IssuedAt = "iat";
    public const string NotBefore = "nbf";
    public const string Expires = "exp";
    public const string AuthenticationLevel = "l";
    public const string AuthenticatedParty = "c";
    public const string DialogParty = "p";
    public const string SupplierParty = "u";
    public const string ServiceResource = "s";
    public const string DialogId = "i";
    public const string Actions = "a";
}
