using System.Buffers.Text;
using System.Text;
using System.Text.Json;
using NSec.Cryptography;

namespace Altinn.ApiClients.Dialogporten.Services;

internal sealed class DialogTokenVerifier : IDialogTokenVerifier
{
    private readonly IEdDsaSecurityKeysCache _publicKeysCache;

    public DialogTokenVerifier(IEdDsaSecurityKeysCache publicKeysCache)
    {
        _publicKeysCache = publicKeysCache;
    }

    public bool Verify(ReadOnlySpan<char> token)
    {
        var publicKeys = _publicKeysCache.PublicKeys;
        if (publicKeys.Count == 0)
        {
            // TODO: Fix this
            throw new InvalidOperationException();
        }

        Span<byte> tokenByteSpan = stackalloc byte[Base64Url.GetMaxDecodedLength(token.Length)];
        if (!TryExtractTokenParts(token, tokenByteSpan, out var headerSpan, out var bodySpan, out var signatureSpan))
        {
            return false;
        }

        var headerAndBodySpan = tokenByteSpan[..(headerSpan.Length + bodySpan.Length + 1)];
        var lala = Encoding.UTF8.GetString(headerAndBodySpan).Split('.');

        if (!SignatureAlgorithm.Ed25519.Verify(publicKeys[1].Key, headerAndBodySpan, signatureSpan))
        {
            return false;
        }

        return false;
        // TODO: Get correct public key based on kid

        // try // Amund: me no like
        // {
        //     if (EdDsaSecurityKeysCacheService.PublicKeys.Count == 0)
        //     {
        //         return false;
        //     }
        //     var publicKeyPair = EdDsaSecurityKeysCacheService.PublicKeys[0];
        //
        //     var tokenPartEnumerator = token.Split('.');
        //     if (!tokenPartEnumerator.MoveNext())
        //     {
        //         return false;
        //     }
        //
        //     // Amund: Sparer ca 3 operations i IL med å lagre i egen variabel først
        //     var current = tokenPartEnumerator.Current;
        //     Span<byte> header = stackalloc byte[current.End.Value - current.Start.Value];
        //     if (!tokenPartEnumerator.MoveNext() || !Base64Url.TryDecodeFromChars(token[current], header, out var headerLength))
        //     {
        //         return false;
        //     }
        //
        //     current = tokenPartEnumerator.Current;
        //     Span<byte> body = stackalloc byte[current.End.Value - current.Start.Value];
        //     if (!tokenPartEnumerator.MoveNext() || !Base64Url.TryDecodeFromChars(token[current], body, out var bodyLength))
        //     {
        //         return false;
        //     }
        //
        //     current = tokenPartEnumerator.Current;
        //     Span<byte> signature = stackalloc byte[current.End.Value - current.Start.Value];
        //     if (tokenPartEnumerator.MoveNext() && !Base64Url.TryDecodeFromChars(token[current], signature, out _))
        //     {
        //         return false;
        //     }
        //     var headerJson = JsonSerializer.Deserialize<JsonElement>(header);
        //     if (!headerJson.TryGetProperty("kid", out var value) && value.GetString() != publicKeyPair.Kid)
        //     {
        //         return false;
        //     }
        //
        //     Span<byte> headerAndBody = stackalloc byte[headerLength + bodyLength + 1];
        //     header[..headerLength].CopyTo(headerAndBody);
        //     headerAndBody[header.Length] = (byte)'.';
        //     body[..bodyLength].CopyTo(headerAndBody[(header.Length + 1)..]);
        //
        //     if (!SignatureAlgorithm.Ed25519.Verify(publicKeyPair.Key, headerAndBody, signature))
        //     {
        //         return false;
        //     }
        //
        //     var bodyJson = JsonSerializer.Deserialize<JsonElement>(body);
        //
        //     return TryGetExpires(bodyJson, out var expiresOffset) &&
        //            expiresOffset >= DateTimeOffset.UtcNow;
        // }
        // catch (FormatException)
        // {
        //     return false;
        // }
    }

    private static bool TryExtractTokenParts(
        ReadOnlySpan<char> token,
        Span<byte> tokenBytes,
        out ReadOnlySpan<byte> header,
        out ReadOnlySpan<byte> body,
        out ReadOnlySpan<byte> signature)
    {
        header = body = signature = default;
        var enumerator = token.Split('.');
        var spanPointer = 0;

        // Header
        if (!enumerator.MoveNext()) return false;
        var spanPointerNext = spanPointer + Base64Url.DecodeFromChars(token[enumerator.Current], tokenBytes[spanPointer..]);
        header = tokenBytes[spanPointer..spanPointerNext];
        tokenBytes[spanPointerNext++] = (byte)'.';
        spanPointer = spanPointerNext;

        // Body
        if (!enumerator.MoveNext()) return false;
        spanPointerNext = spanPointer + Base64Url.DecodeFromChars(token[enumerator.Current], tokenBytes[spanPointer..]);
        body = tokenBytes[spanPointer..spanPointerNext];
        tokenBytes[spanPointerNext++] = (byte)'.';
        spanPointer = spanPointerNext;

        // Signature
        if (!enumerator.MoveNext()) return false;
        spanPointerNext = spanPointer + Base64Url.DecodeFromChars(token[enumerator.Current], tokenBytes[spanPointer..]);
        signature = tokenBytes[spanPointer..spanPointerNext];

        return !enumerator.MoveNext();
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
