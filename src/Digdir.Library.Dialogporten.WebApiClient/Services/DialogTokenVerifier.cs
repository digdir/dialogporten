using System.Buffers.Text;
using System.Text.Json;
using Altinn.ApiClients.Dialogporten.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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

            var headerJson = JsonSerializer.Deserialize<JsonElement>(header);
            if (!headerJson.TryGetProperty("kid", out var value) && value.GetString() != _kid)
            {
                return false;
            }

            Span<byte> headerAndBody = stackalloc byte[headerLength + bodyLength + 1];
            header[..headerLength].CopyTo(headerAndBody);
            headerAndBody[header.Length] = (byte)'.';
            body[..bodyLength].CopyTo(headerAndBody[(header.Length + 1)..]);

            if (!SignatureAlgorithm.Ed25519.Verify(_publicKey, headerAndBody, signature))
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

internal sealed class JsonWebKeyTypes
{
    public const string Kty = "kty";
    public const string Use = "use";
    public const string Kid = "kid";
    public const string Crv = "crv";
    public const string X = "x";
    public const string Alg = "alg";
}

internal class EdDsaSecurityKeysCacheService : IHostedService, IDisposable
{
    public static List<PublicKey> PublicKeys => _keys;
    private static volatile List<PublicKey> _keys = new();

    private PeriodicTimer? _timer;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EdDsaSecurityKeysCacheService> _logger;

    private readonly TimeSpan _refreshInterval = TimeSpan.FromHours(12);

    // In this service we allow keys for all non-production environments for
    // simplicity. Usually one would only allow a single environment (issuer) here,
    // which we could get from an injected IConfiguration/IOptions
    private readonly List<string> _wellKnownEndpoints =
    [
        //"https://localhost:7214/api/v1/.well-known/jwks.json",
        "https://altinn-dev-api.azure-api.net/dialogporten/api/v1/.well-known/jwks.json",
        "https://platform.tt02.altinn.no/dialogporten/api/v1/.well-known/https://altinn-dev-api.azure-api.net/dialogporten/api/v1/.well-known/jwks.jsonhttps://altinn-dev-api.azure-api.net/dialogporten/api/v1/.well-known/jwks.jsonjwks.json"
    ];

    public EdDsaSecurityKeysCacheService(IHttpClientFactory httpClientFactory, ILogger<EdDsaSecurityKeysCacheService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            _timer = new PeriodicTimer(_refreshInterval);
            while (await _timer.WaitForNextTickAsync(cancellationToken))
            {
                try
                {
                    await RefreshAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while refreshing the EdDsa keys.");
                }
            }
        }, cancellationToken);

        await RefreshAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var keys = new List<PublicKey>();

        foreach (var endpoint in _wellKnownEndpoints)
        {
            try
            {
                var response = await httpClient.GetStringAsync(endpoint, cancellationToken);
                var jwks = JsonSerializer.Deserialize<JsonElement>(response);

                if (!jwks.TryGetProperty(JsonWebKeyTypes.X, out var publicKey) || !jwks.TryGetProperty(JsonWebKeyTypes.Alg, out var alg))
                {
                    continue;
                }

                if (alg.GetString() == "EdDSA")
                {

                    keys.Add(PublicKey.Import(
                        SignatureAlgorithm.Ed25519,
                        Base64Url.DecodeFromChars(publicKey.GetString()),
                        KeyBlobFormat.RawPublicKey));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve keys from {endpoint}", endpoint);
            }
        }

        _logger.LogInformation("Refreshed EdDsa keys cache with {count} keys", keys.Count);

        var newKeys = keys.ToList();
        _keys = newKeys; // Atomic replace
    }
}
