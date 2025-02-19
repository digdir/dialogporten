using System.Buffers.Text;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Altinn.ApiClients.Dialogporten.Common;
using Altinn.ApiClients.Dialogporten.Services;
using NSec.Cryptography;
using NSubstitute;

namespace Digdir.Library.Dialogporten.WebApiClient.Unit.Tests;

public class DialogTokenValidatorTests
{
    private const string ValidTimeStampString = "2025-02-14T09:00:00Z";
    private const string DialogToken =
        "eyJhbGciOiJFZERTQSIsInR5cCI6IkpXVCIsImtpZCI6ImRwLXN0YWdpbmctMjQwMzIyLW81eW1uIn0.eyJqdGkiOiIzOGNmZGNiOS0zODhiLTQ3YjgtYTFiZi05ZjE1YjI4MTk4OTQiLCJjIjoidXJuOmFsdGlubjpwZXJzb246aWRlbnRpZmllci1ubzoxNDg4NjQ5ODIyNiIsImwiOjMsInAiOiJ1cm46YWx0aW5uOnBlcnNvbjppZGVudGlmaWVyLW5vOjE0ODg2NDk4MjI2IiwicyI6InVybjphbHRpbm46cmVzb3VyY2U6ZGFnbC1jb3JyZXNwb25kZW5jZSIsImkiOiIwMTk0ZmU4Mi05MjgwLTc3YTUtYTdjZC01ZmYwZTZhNmZhMDciLCJhIjoicmVhZCIsImlzcyI6Imh0dHBzOi8vcGxhdGZvcm0udHQwMi5hbHRpbm4ubm8vZGlhbG9ncG9ydGVuL2FwaS92MSIsImlhdCI6MTczOTUyMzM2NywibmJmIjoxNzM5NTIzMzY3LCJleHAiOjE3Mzk1MjM5Njd9.O_f-RJhRPT7B76S7aOGw6jfxKDki3uJQLLC8nVlcNVJWFIOQUsy6gU4bG1ZdqoMBZPvb2K2X4I5fGpHW9dQMAA";
    private static readonly PublicKeyPair[] ValidPublicKeyPairs =
    [
        new("dp-staging-240322-o5ymn", ToPublicKey("zs9hR9oqgf53th2lTdrBq3C1TZ9UlR-HVJOiUpWV63o")),
        new("dp-staging-240322-rju3g", ToPublicKey("23Sijekv5ATW4sSEiRPzL_rXH-zRV8MK8jcs5ExCmSU"))
    ];

    [Fact]
    public void ShouldReturnIsValid_GivenValidToken()
    {
        // Arrange
        var sut = GetSut(
            DateTimeOffset.Parse(ValidTimeStampString, CultureInfo.InvariantCulture),
            ValidPublicKeyPairs);

        // Act
        var result = sut.Validate(DialogToken);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ShouldThrowException_GivenNoPublicKeys()
    {

        // Arrange
        var sut = GetSut(
            DateTimeOffset.Parse(ValidTimeStampString, CultureInfo.InvariantCulture));

        // Assert
        Assert.Throws<InvalidOperationException>(() => sut.Validate(DialogToken));
    }

    [Fact]
    public void ShouldReturnError_GivenMalformedToken()
    {
        // Arrange
        var sut = GetSut(
            DateTimeOffset.Parse(ValidTimeStampString, CultureInfo.InvariantCulture),
            ValidPublicKeyPairs);

        // Act
        var result = sut.Validate("This.TokenIsMalformed....");

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.ContainsKey("token"));
        Assert.Contains("Invalid token format", result.Errors["token"]);
    }

    [Fact]
    public void ShouldReturnError_GivenInvalidToken()
    {
        // Arrange
        var sut = GetSut(
            DateTimeOffset.Parse(ValidTimeStampString, CultureInfo.InvariantCulture),
            ValidPublicKeyPairs);

        // Act
        var result = sut.Validate("This.TokenIs.Invalid");

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.ContainsKey("token"));
        Assert.Contains("Invalid token format", result.Errors["token"]);
    }
    [Fact]
    public void ShouldReturnError_GivenNoPublicKeyWithCorrectKeyId()
    {
        // Arrange
        var sut = GetSut(
            DateTimeOffset.Parse(ValidTimeStampString, CultureInfo.InvariantCulture),
            ValidPublicKeyPairs);

        var token = UpdateTokenHeader(DialogToken, "kid", "dp-testing-fake-kid");
        // Act
        var result = sut.Validate(token);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.ContainsKey("token"));
        Assert.Contains("Invalid signature", result.Errors["token"]);
    }

    [Fact]
    public void ShouldReturnError_GivenExpiredToken()
    {
        // Arrange
        var sut = GetSut(
            DateTimeOffset.Parse("2025-02-17T09:00:00Z", CultureInfo.InvariantCulture),
            ValidPublicKeyPairs);

        // Act
        var result = sut.Validate(DialogToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.ContainsKey("token"));
        Assert.Contains("Token has expired", result.Errors["token"]);
    }

    [Fact]
    public void ShouldReturnError_GivenEmptyToken()
    {
        // Arrange
        var sut = GetSut(
            DateTimeOffset.Parse(ValidTimeStampString, CultureInfo.InvariantCulture),
            ValidPublicKeyPairs);

        // Act
        var result = sut.Validate("");

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.ContainsKey("token"));
        Assert.Contains("Invalid token format", result.Errors["token"]);
    }

    [Fact]
    public void ShouldReturnError_GivenTokenWithWrongSignature()
    {
        // Arrange
        var sut = GetSut(
            DateTimeOffset.Parse(ValidTimeStampString, CultureInfo.InvariantCulture),
            ValidPublicKeyPairs);

        var token = UpdateTokenPayload(DialogToken, "l", "4");

        // Act
        var result = sut.Validate(token);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.ContainsKey("token"));
        Assert.Contains("Invalid signature", result.Errors["token"]);
    }

    [Fact]
    public void ShouldReturnError_GivenTokenWithWrongAlg()
    {
        // Arrange
        var sut = GetSut(
            DateTimeOffset.Parse(ValidTimeStampString, CultureInfo.InvariantCulture),
            ValidPublicKeyPairs);

        var token = UpdateTokenHeader(DialogToken, "alg", "RS512");

        // Act
        var result = sut.Validate(token);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.ContainsKey("token"));
        Assert.Contains("Invalid signature", result.Errors["token"]);
    }

    [Fact]
    public void ShouldReturnError_GivenMalformedJsonHeader()
    {
        var invalidHeader = """
                            {
                              "alg": "EdDSA",
                              "typ": "JWT",
                              "kid": "dp-staging-240322-o5ymn"
                            """u8;
        // Arrange
        var sut = GetSut(
            DateTimeOffset.Parse(ValidTimeStampString, CultureInfo.InvariantCulture),
            ValidPublicKeyPairs);

        var tokenParts = DialogToken.Split('.');
        tokenParts[0] = Base64Url.EncodeToString(invalidHeader);
        var token = string.Join(".", tokenParts);

        // Act
        var result = sut.Validate(token);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.ContainsKey("token"));
        Assert.Contains("Invalid token format", result.Errors["token"]);
    }

    [Fact]
    public void ShouldReturnError_GivenMalformedJsonBody()
    {
        var invalidBody = """
                          {
                            "jti": "38cfdcb9-388b-47b8-a1bf-9f15b2819894",
                            "c": "urn:altinn:person:identifier-no:14886498226",
                          """u8;
        // Arrange
        var sut = GetSut(
            DateTimeOffset.Parse(ValidTimeStampString, CultureInfo.InvariantCulture),
            ValidPublicKeyPairs);

        var tokenParts = DialogToken.Split('.');
        tokenParts[1] = Base64Url.EncodeToString(invalidBody);
        var token = string.Join(".", tokenParts);

        // Act
        var result = sut.Validate(token);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.ContainsKey("token"));
        Assert.Contains("Invalid token format", result.Errors["token"]);
    }

    private static DialogTokenValidator GetSut(DateTimeOffset simulatedNow, params PublicKeyPair[] publicKeyPairs)
    {
        var keyCache = Substitute.For<IEdDsaSecurityKeysCache>();
        var clock = Substitute.For<IClock>();
        keyCache.PublicKeys.Returns(new ReadOnlyCollection<PublicKeyPair>(publicKeyPairs));
        clock.UtcNow.Returns(simulatedNow);
        return new DialogTokenValidator(keyCache, clock);
    }

    private static PublicKey ToPublicKey(string key)
        => PublicKey.Import(SignatureAlgorithm.Ed25519, Base64Url.DecodeFromChars(key), KeyBlobFormat.RawPublicKey);

    private static string UpdateTokenParts(string part, string property, string value)
    {
        var decodedPart = Base64Url.DecodeFromChars(part);
        var json = JsonSerializer.Deserialize<Dictionary<string, object>>(decodedPart)!;
        json[property] = value;
        var encodedPart = Base64Url.EncodeToUtf8(JsonSerializer.SerializeToUtf8Bytes(json));
        return Encoding.UTF8.GetString(encodedPart);
    }

    private static string UpdateTokenPayload(string token, string property, string value)
    {
        var tokenParts = token.Split('.');
        tokenParts[1] = UpdateTokenParts(tokenParts[1], property, value);
        return string.Join(".", tokenParts);
    }

    private static string UpdateTokenHeader(string token, string property, string value)
    {
        var tokenParts = token.Split('.');
        tokenParts[0] = UpdateTokenParts(tokenParts[0], property, value);
        return string.Join(".", tokenParts);
    }
}
