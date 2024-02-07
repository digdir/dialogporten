using System.Security.Cryptography;
using System.Text;
using BenchmarkDotNet.Attributes;
using Org.BouncyCastle.Crypto.Signers;
using NSec.Cryptography;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using NsecEd25519 = NSec.Cryptography.Ed25519;

namespace Digdir.Tool.Dialogporten.Benchmarks;

[MemoryDiagnoser]
public class TokenGenerationBenchmark
{
    private RSA _rsa = null!;
    private ECDsa _ecdsa = null!;
    private Ed25519PrivateKeyParameters _eddsaKeys = null!;
    private Ed25519Signer _eddsaSigner = null!;
    private NsecEd25519 _nSecAlgorithm = null!;
    private Key _nSecKey = null!;

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

        // For correctness checks, use a pre-defined key in EdDSA signatures
        const string ed25519Pkcs8 = """
                                    -----BEGIN PRIVATE KEY-----
                                    MC4CAQAwBQYDK2VwBCIEIAYIsKL0xkTkAXDhUN6eDheqODEOGyFZ04jsgFNCFxZf
                                    -----END PRIVATE KEY-----
                                    """;
        // EdDSA Setup using Ed25519 (gives security equivalent to a 128 bit symmetric key)
        var pemReaderPrivate = new PemReader(new StringReader(ed25519Pkcs8));
        _eddsaKeys = (Ed25519PrivateKeyParameters)pemReaderPrivate.ReadObject();
        _eddsaSigner = new Ed25519Signer();
        _eddsaSigner.Init(true, _eddsaKeys);

        // Setup for NSec
        _nSecAlgorithm = SignatureAlgorithm.Ed25519;
        _nSecKey = Key.Import(_nSecAlgorithm, Encoding.UTF8.GetBytes(ed25519Pkcs8), KeyBlobFormat.PkixPrivateKeyText);

        var r1 = GenerateTokenWithEdDsaBouncyCastle();
        var r2 = GenerateTokenWithEdDsaNSec();

        if (!r1.SequenceEqual(r2))
        {
            Console.WriteLine("BouncyCastle: " + BitConverter.ToString(r1));
            Console.WriteLine("        NSec: " + BitConverter.ToString(r2));
            throw new InvalidOperationException();
        }
    }

    [Benchmark]
    public byte[] GenerateTokenWithRsa()
    {
        return _rsa.SignData(_payload, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }

    [Benchmark]
    public byte[] GenerateTokenWithEcdsa()
    {
        return _ecdsa.SignData(_payload, HashAlgorithmName.SHA256);
    }

    [Benchmark]
    public byte[] GenerateTokenWithEdDsaBouncyCastle()
    {
        _eddsaSigner.Init(true, _eddsaKeys);
        _eddsaSigner.BlockUpdate(_payload, 0, _payload.Length);
        return _eddsaSigner.GenerateSignature();
    }

    [Benchmark]
    public byte[] GenerateTokenWithEdDsaNSec()
    {
        return _nSecAlgorithm.Sign(_nSecKey, _payload);
    }

    private static string Base64UrlEncode(string input)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(input))
                      .TrimEnd('=')
                      .Replace('+', '-')
                      .Replace('/', '_');
    }
}
