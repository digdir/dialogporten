using System.Buffers.Text;
using System.Collections.ObjectModel;
using System.Globalization;
using Altinn.ApiClients.Dialogporten.Common;
using Altinn.ApiClients.Dialogporten.Services;
using NSec.Cryptography;
using NSubstitute;

namespace Digdir.Library.Dialogporten.WebApiClient.Unit.Tests;

public class DialogTokenVerifierTests
{
    [Fact]
    public void Test1()
    {
        // Arrange
        const string dialogToken = "eyJhbGciOiJFZERTQSIsInR5cCI6IkpXVCIsImtpZCI6ImRwLXN0YWdpbmctMjQwMzIyLW81eW1uIn0.eyJqdGkiOiIzOGNmZGNiOS0zODhiLTQ3YjgtYTFiZi05ZjE1YjI4MTk4OTQiLCJjIjoidXJuOmFsdGlubjpwZXJzb246aWRlbnRpZmllci1ubzoxNDg4NjQ5ODIyNiIsImwiOjMsInAiOiJ1cm46YWx0aW5uOnBlcnNvbjppZGVudGlmaWVyLW5vOjE0ODg2NDk4MjI2IiwicyI6InVybjphbHRpbm46cmVzb3VyY2U6ZGFnbC1jb3JyZXNwb25kZW5jZSIsImkiOiIwMTk0ZmU4Mi05MjgwLTc3YTUtYTdjZC01ZmYwZTZhNmZhMDciLCJhIjoicmVhZCIsImlzcyI6Imh0dHBzOi8vcGxhdGZvcm0udHQwMi5hbHRpbm4ubm8vZGlhbG9ncG9ydGVuL2FwaS92MSIsImlhdCI6MTczOTUyMzM2NywibmJmIjoxNzM5NTIzMzY3LCJleHAiOjE3Mzk1MjM5Njd9.O_f-RJhRPT7B76S7aOGw6jfxKDki3uJQLLC8nVlcNVJWFIOQUsy6gU4bG1ZdqoMBZPvb2K2X4I5fGpHW9dQMAA";
        var keyCache = CreateEdDsaSecurityKeysCacheMock([
            new("dp-staging-240322-o5ymn", ToPublicKey("zs9hR9oqgf53th2lTdrBq3C1TZ9UlR-HVJOiUpWV63o")),
            new("dp-staging-240322-rju3g", ToPublicKey("23Sijekv5ATW4sSEiRPzL_rXH-zRV8MK8jcs5ExCmSU")),
        ]);
        var clock = Substitute.For<IClock>();
        clock.UtcNow.Returns(DateTimeOffset.Parse("2025-02-14T09:00:00Z", CultureInfo.InvariantCulture));
        var sut = new DialogTokenVerifier(keyCache, clock);

        // Act
        var result = sut.Validate(dialogToken);

        // Assert
        Assert.True(result.IsValid);
    }

    private static IEdDsaSecurityKeysCache CreateEdDsaSecurityKeysCacheMock(IEnumerable<PublicKeyPair> publicKeyPairs)
    {
        var keyCache = Substitute.For<IEdDsaSecurityKeysCache>();
        keyCache.PublicKeys.Returns(new ReadOnlyCollection<PublicKeyPair>(publicKeyPairs.ToList()));
        return keyCache;
    }

    private static PublicKey ToPublicKey(string key)
        => PublicKey.Import(SignatureAlgorithm.Ed25519, Base64Url.DecodeFromChars(key), KeyBlobFormat.RawPublicKey);
}
