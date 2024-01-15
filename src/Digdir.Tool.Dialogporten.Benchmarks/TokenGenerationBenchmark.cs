using System.Security.Cryptography;
using System.Text;
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
    private Ed25519Signer _eddsaSigner = null!;

    private readonly byte[] _payload = Encoding.UTF8.GetBytes(Base64UrlEncode(
        """
          {
              "l": 4,
              "c": "urn:altinn:person:identifier-no::12018212345",
              "p": "urn:altinn:organization:identifier-no::991825827",
              "s": "urn:altinn:organization:identifier-no::825827991",
              "i": "e0300961-85fb-4ef2-abff-681d77f9960e",
              "u": "https://example.com/api/dialogs/123456789/dialogelements/5b5446a7.pdf",
              "exp": 1672772834,
              "iss": "https://dialogporten.no",
              "nbf": 1672771934,
              "iat": 1672771934
          }
          """));
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
        _eddsaSigner = new Ed25519Signer();
        _eddsaSigner.Init(true, _eddsaKeys.Private);
    }

    [Benchmark]
    public void GenerateTokenWithRsa()
    {
        _rsa.SignData(_payload, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }

    [Benchmark]
    public void GenerateTokenWithEcdsa()
    {
        _ecdsa.SignData(_payload, HashAlgorithmName.SHA256);
    }

    [Benchmark]
    public void GenerateTokenWithEdDsa()
    {
        _eddsaSigner.Init(true, _eddsaKeys.Private);
        _eddsaSigner.BlockUpdate(_payload, 0, _payload.Length);
        _eddsaSigner.GenerateSignature();
    }

    private static string Base64UrlEncode(string input)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(input))
                      .TrimEnd('=')
                      .Replace('+', '-')
                      .Replace('/', '_');
    }
}
