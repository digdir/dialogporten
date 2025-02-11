using System.Buffers.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSec.Cryptography;

namespace Altinn.ApiClients.Dialogporten.Services;

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

                if (!jwks.TryGetProperty("keys", out var keysElement))
                {
                    continue;
                }
                foreach (var jwk in keysElement.EnumerateArray())
                {

                    if (!jwk.TryGetProperty(JsonWebKeyTypes.X, out var publicKey) || !jwk.TryGetProperty(JsonWebKeyTypes.Alg, out var alg))
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

internal sealed class JsonWebKeyTypes
{
    public const string Kty = "kty";
    public const string Use = "use";
    public const string Kid = "kid";
    public const string Crv = "crv";
    public const string X = "x";
    public const string Alg = "alg";
}
