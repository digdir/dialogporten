using System.Buffers.Text;
using System.Collections.ObjectModel;
using System.Net;
using Altinn.ApiClients.Dialogporten.Config;
using Altinn.ApiClients.Dialogporten.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSec.Cryptography;

namespace Altinn.ApiClients.Dialogporten.Services;

internal sealed class EdDsaSecurityKeysCacheService : BackgroundService
{
    public static ReadOnlyCollection<PublicKeyPair> PublicKeys => _keys.AsReadOnly();
    private static volatile List<PublicKeyPair> _keys = [];

    private readonly PeriodicTimer _timer = new(TimeSpan.FromHours(12));
    private readonly ILogger<EdDsaSecurityKeysCacheService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private const string EdDsaAlg = "EdDSA";

    public EdDsaSecurityKeysCacheService(ILogger<EdDsaSecurityKeysCacheService> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
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

        await RefreshAsync(cancellationToken);
    }

    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dialogportenApi = scope.ServiceProvider.GetRequiredService<IInternalDialogportenApi>();
        var jwks = await dialogportenApi.GetJwks(cancellationToken);

        var keys = jwks.Keys
            .Where(x => StringComparer.OrdinalIgnoreCase.Equals(x.Alg, EdDsaAlg))
            .Select(k => new PublicKeyPair(
                PublicKey.Import(
                    SignatureAlgorithm.Ed25519,
                    Base64Url.DecodeFromChars(k.X),
                    KeyBlobFormat.RawPublicKey),
                k.Kid))
            .ToList();

        _keys = keys;
    }

    public override void Dispose()
    {
        _timer.Dispose();
        base.Dispose();
    }
}

internal sealed record PublicKeyPair(PublicKey Key, string Kid);
