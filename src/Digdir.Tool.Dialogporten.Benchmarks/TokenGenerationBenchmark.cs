using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Signers;

namespace Digdir.Tool.Dialogporten.Benchmarks;

[MemoryDiagnoser]
public class TokenGenerationBenchmark
{
    private RSA _rsa = null!;
    private ECDsa _ecdsa = null!;
    private AsymmetricCipherKeyPair _eddsaKeys = null!;

    private readonly JsonDocument _claims = JsonDocument.Parse(@"
        {
            ""l"": 4,
            ""c"": ""urn:altinn:person:identifier-no::12018212345"",
            ""p"": ""urn:altinn:organization:identifier-no::991825827"",
            ""s"": ""urn:altinn:organization:identifier-no::825827991"",
            ""i"": ""e0300961-85fb-4ef2-abff-681d77f9960e"",
            ""u"": ""https://example.com/api/dialogs/123456789/dialogelements/5b5446a7.pdf"",
            ""exp"": 1672772834,
            ""iss"": ""https://dialogporten.no"",
            ""nbf"": 1672771934,
            ""iat"": 1672771934 
        }");

    [GlobalSetup]
    public void Setup()
    {
        // RSA Setup (gives security equivalent to a 112 bit symmetric key)
        _rsa = RSA.Create();
        _rsa.KeySize = 2048;

        // ECDSA Setup (gives security equivalent to a 128 bit symmetric key)
        _ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        // EdDSA Setup using Ed25519 (gives security equivalent to a 128 bit symmetric key)
        var generator = new Ed25519KeyPairGenerator();
        generator.Init(new KeyGenerationParameters(new SecureRandom(), 256));
        _eddsaKeys = generator.GenerateKeyPair();
    }

    [Benchmark]
    public string GenerateTokenWithRsa()
    {
        var header = JsonSerializer.Serialize(new
        {
            alg = "RS256",
            typ = "JWT"
        });

        var payload = JsonSerializer.Serialize(_claims);

        return GenerateJwtToken(header, payload, _rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }

    [Benchmark]
    public string GenerateTokenWithEcdsa()
    {
        var header = JsonSerializer.Serialize(new
        {
            alg = "ES256",
            typ = "JWT"
        });

        var payload = JsonSerializer.Serialize(_claims);

        return GenerateJwtToken(header, payload, _ecdsa, HashAlgorithmName.SHA256);
    }

    [Benchmark]
    public string GenerateTokenWithEdDsa()
    {
        // Prepare EdDSA token
        var signer = new Ed25519Signer();
        signer.Init(true, _eddsaKeys.Private);

        // Serialize claims to JSON
        var claimsJson = JsonSerializer.Serialize(_claims);

        // Sign the data
        var data = Encoding.UTF8.GetBytes(claimsJson);
        signer.BlockUpdate(data, 0, data.Length);
        var signature = signer.GenerateSignature();

        // Construct the token (you need to define the exact format)
        return Base64UrlEncode(signature);
    }

    private static string GenerateJwtToken(string header, string payload, RSA rsa, HashAlgorithmName hashAlgorithm, RSASignaturePadding padding)
    {
        var headerBase64 = Base64UrlEncode(header);
        var payloadBase64 = Base64UrlEncode(payload);

        var data = Encoding.UTF8.GetBytes($"{headerBase64}.{payloadBase64}");

        var signature = rsa.SignData(data, hashAlgorithm, padding);
        var signatureBase64 = Base64UrlEncode(signature);

        return $"{headerBase64}.{payloadBase64}.{signatureBase64}";
    }

    private static string GenerateJwtToken(string header, string payload, ECDsa ecdsa, HashAlgorithmName hashAlgorithm)
    {
        var headerBase64 = Base64UrlEncode(header);
        var payloadBase64 = Base64UrlEncode(payload);

        var data = Encoding.UTF8.GetBytes($"{headerBase64}.{payloadBase64}");

        var signature = ecdsa.SignData(data, hashAlgorithm);
        var signatureBase64 = Base64UrlEncode(signature);

        return $"{headerBase64}.{payloadBase64}.{signatureBase64}";
    }

    private static string Base64UrlEncode(string input)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        return Base64UrlEncode(inputBytes);
    }

    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
                      .TrimEnd('=')
                      .Replace('+', '-')
                      .Replace('/', '_');
    }
}
