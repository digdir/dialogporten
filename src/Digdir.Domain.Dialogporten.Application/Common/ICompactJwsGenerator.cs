using System.Text.Json;
using Microsoft.Extensions.Options;
using NSec.Cryptography;
using System.Buffers.Text;
using System.Text;
using System.Text.Json.Serialization;

namespace Digdir.Domain.Dialogporten.Application.Common;

public interface ICompactJwsGenerator
{
    string GetCompactJws(DialogTokenClaims claims);
}

internal class Ed25519Generator : ICompactJwsGenerator
{
    private readonly ApplicationSettings _applicationSettings;
    private string? _kid;
    private Key? _key;
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public Ed25519Generator(IOptions<ApplicationSettings> applicationSettings)
    {
        _applicationSettings = applicationSettings.Value;
    }

    public string GetCompactJws(DialogTokenClaims claims)
    {
        InitSigningKey();

        var header = JsonSerializer.SerializeToUtf8Bytes(new
        {
            alg = "EdDSA",
            typ = "JWT",
            kid = _kid
        });

        var payload = JsonSerializer.SerializeToUtf8Bytes(claims, _serializerOptions);

        Span<byte> buffer = stackalloc byte[Base64Url.GetMaxEncodedToUtf8Length(header.Length)
                                            // 64 for separators and signature
                                            + Base64Url.GetMaxEncodedToUtf8Length(payload.Length) + 64];

        Base64Url.Encode(header, buffer, out var bytesWritten);
        var encodedHeader = Encoding.UTF8.GetString(buffer[..bytesWritten]);

        Base64Url.Encode(payload, buffer, out bytesWritten);
        var encodedPayload = Encoding.UTF8.GetString(buffer[..bytesWritten]);

        var dataToSign = $"{encodedHeader}.{encodedPayload}";
        var signature = SignatureAlgorithm.Ed25519.Sign(_key!, Encoding.UTF8.GetBytes(dataToSign));

        Base64Url.Encode(signature, buffer, out bytesWritten);
        var encodedSignature = Encoding.UTF8.GetString(buffer[..bytesWritten]);

        return $"{encodedHeader}.{encodedPayload}.{encodedSignature}";
    }

    private void InitSigningKey()
    {
        if (_kid != null) return;

        var keyPair = _applicationSettings.Dialogporten.Ed25519KeyPairs.Primary;
        _kid = keyPair.Kid;
        _key = Key.Import(SignatureAlgorithm.Ed25519,
            Base64Url.Decode(keyPair.PrivateComponent), KeyBlobFormat.RawPrivateKey);
    }
}

public static class Base64Url
{
    public static int GetMaxEncodedToUtf8Length(int length) => (length + 2) / 3 * 4;

    public static void Encode(ReadOnlySpan<byte> data, Span<byte> destination, out int written)
    {
        Base64.EncodeToUtf8(data, destination, out _, out written);
        for (var i = 0; i < written; i++)
        {
            destination[i] = destination[i] switch
            {
                (byte)'+' => (byte)'-',
                (byte)'/' => (byte)'_',
                _ => destination[i]
            };
        }
        while (written > 0 && destination[written - 1] == '=') written--;
    }

    public static byte[] Decode(string input)
    {
        var output = input;
        output = output.Replace('-', '+').Replace('_', '/');
        switch (output.Length % 4)
        {
            case 0: break;
            case 2: output += "=="; break;
            case 3: output += "="; break;
            default: throw new ArgumentException("Illegal base64url string", nameof(input));
        }
        return Convert.FromBase64String(output);
    }
}
