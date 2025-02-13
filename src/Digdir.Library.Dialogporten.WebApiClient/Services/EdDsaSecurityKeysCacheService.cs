using System.Buffers.Text;
using System.Net;
using Altinn.ApiClients.Dialogporten.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSec.Cryptography;

namespace Altinn.ApiClients.Dialogporten.Services;

internal class EdDsaSecurityKeysCacheService : BackgroundService
{
    public static List<PublicKey> PublicKeys => _keys;
    private static volatile List<PublicKey> _keys = [];

    private PeriodicTimer? _timer;
    private readonly ILogger<EdDsaSecurityKeysCacheService> _logger;

    private readonly TimeSpan _refreshInterval = TimeSpan.FromHours(12);
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private const string EdDsaAlg = "EdDSA";

    public EdDsaSecurityKeysCacheService(ILogger<EdDsaSecurityKeysCacheService> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
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

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Dispose();
        base.Dispose();
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _timer?.Dispose();
        base.Dispose();
    }

    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dialogportenApi = scope.ServiceProvider.GetRequiredService<IInternalDialogportenApi>();
        var response = await dialogportenApi.GetJwks(cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK || response.Content == null)
        {
            return;
        }

        var keys = response.Content.Keys
            .Where(x => StringComparer.OrdinalIgnoreCase.Equals(x.Alg, EdDsaAlg))
            .Select(k => PublicKey.Import(
                SignatureAlgorithm.Ed25519,
                Base64Url.DecodeFromChars(k.X),
                KeyBlobFormat.RawPublicKey))
            .ToList();

        _keys = keys;
    }
}
