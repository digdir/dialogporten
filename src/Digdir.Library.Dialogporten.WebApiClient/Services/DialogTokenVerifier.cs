using System.Buffers.Text;
using System.Text;
using System.Text.Json;
using Altinn.ApiClients.Dialogporten.Config;
using Microsoft.Extensions.Configuration;
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
    private readonly PublicKey _publicKey;
    public DialogTokenVerifier(IOptions<DialogportenSettings> options)
    {
        _kid = options.Value.Ed25519Keys.Primary.Kid;
        _publicKey = PublicKey.Import(SignatureAlgorithm.Ed25519,
            Base64Url.DecodeFromChars(options.Value.Ed25519Keys.Primary.PublicComponent), KeyBlobFormat.RawPublicKey);
    }
    public bool Verify(ReadOnlySpan<char> token)
    {
        var tokenPartEnumerator = token.Split('.');
        if (!tokenPartEnumerator.MoveNext())
        {
            return false;
        }

        var header = Base64Url.DecodeFromChars(token[tokenPartEnumerator.Current]);
        if (!tokenPartEnumerator.MoveNext())
        {
            return false;
        }

        var body = Base64Url.DecodeFromChars(token[tokenPartEnumerator.Current]);
        if (!tokenPartEnumerator.MoveNext())
        {
            return false;
        }

        var signature = Base64Url.DecodeFromChars(token[tokenPartEnumerator.Current]);
        if (tokenPartEnumerator.MoveNext())
        {
            return false;
        }


        var headerJson = JsonSerializer.Deserialize<JsonElement>(header);

        if (!headerJson.TryGetProperty("kid", out var value) && value.GetString() != _kid)
        {
            return false;
        }

        var headerAndBody = header
            .Append((byte)'.')
            .Concat(body)
            .ToArray();

        if (!SignatureAlgorithm.Ed25519.Verify(_publicKey, headerAndBody, signature))
        {
            return false;
        }

        var bodyJson = JsonSerializer.Deserialize<JsonElement>(body);

        return TryGetExpires(bodyJson, out var expiresOffset) &&
               expiresOffset >= DateTimeOffset.UtcNow;
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
