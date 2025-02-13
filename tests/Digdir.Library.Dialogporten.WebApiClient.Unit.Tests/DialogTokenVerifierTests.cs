using System.Buffers.Text;
using System.Collections.ObjectModel;
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
        const string dialogToken = "eyJhbGciOiJFZERTQSIsInR5cCI6IkpXVCIsImtpZCI6ImRldi1wcmltYXJ5LXNpZ25pbmcta2V5In0.eyJqdGkiOiI1MDk1YzdiMC0xODA5LTRkNDMtODE4ZC1lYTkzYWJmMmQ5NGQiLCJjIjoidXJuOmFsdGlubjpwZXJzb246aWRlbnRpZmllci1ubzowODg5NTY5OTY4NCIsImwiOjMsInAiOiJ1cm46YWx0aW5uOm9yZ2FuaXphdGlvbjppZGVudGlmaWVyLW5vOjMxMzM0NTQ3NSIsInMiOiJ1cm46YWx0aW5uOnJlc291cmNlOnN1cGVyLXNpbXBsZS1zZXJ2aWNlIiwiaSI6IjAxOTRmZjZkLWRjOGMtNzdhNC04YTU0LWIwMDQxZDNiNDlmNiIsImEiOiJyZWFkIiwiaXNzIjoiaHR0cHM6Ly9sb2NhbGhvc3Q6NzIxNC9hcGkvdjEiLCJpYXQiOjE3Mzk0NTIxOTUsIm5iZiI6MTczOTQ1MjE5NSwiZXhwIjoxNzM5NDUyNzk1fQ.9Ii2mgqaeIV5bdSJchk00liajmdN8qWXjpyREKDyrGREV7DVE52v98Br8UxBt791sbvNXgTeZAdbDzS_MH7UBg";
        var keyCache = CreateEdDsaSecurityKeysCacheMock([
            new("dev-primary-signing-key", ToPublicKey("C8XkQ28jpOrKao-rI6m8qc4BqfkKBgBTll05az183Dg")),
            new("dev-secondary-signing-key", ToPublicKey("OlP9jLxmFccltkzeiQerR4k2P3yWq5nKO3V9VXq5IvY")),
        ]);
        var sut = new DialogTokenVerifier(keyCache);

        // Act
        var result = sut.Verify(dialogToken);

        // Assert
        Assert.True(result);
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
