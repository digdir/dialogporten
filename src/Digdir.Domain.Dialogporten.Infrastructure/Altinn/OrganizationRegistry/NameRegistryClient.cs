using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Digdir.Domain.Dialogporten.Application.Externals;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using ZiggyCreatures.Caching.Fusion;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.NameRegistry;

internal class NameRegistryClient : INameRegistry
{
    private static readonly DistributedCacheEntryOptions OneDayCacheDuration = new() { AbsoluteExpiration = DateTimeOffset.UtcNow.AddDays(1) };
    private static readonly DistributedCacheEntryOptions ZeroCacheDuration = new() { AbsoluteExpiration = DateTimeOffset.MinValue };

    private readonly IFusionCache _cache;
    private readonly HttpClient _client;
    private readonly ILogger<NameRegistryClient> _logger;

    private static readonly JsonSerializerOptions SerializerOptions = new()

    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    public NameRegistryClient(HttpClient client, IFusionCacheProvider cacheProvider, ILogger<NameRegistryClient> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _cache = cacheProvider.GetCache(nameof(NameRegistry)) ?? throw new ArgumentNullException(nameof(cacheProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string?> GetName(string personalIdentificationNumber, CancellationToken cancellationToken)
    {
        return await _cache.GetOrSetAsync(
            $"Name_{personalIdentificationNumber}",
            ct => GetNameFromRegister(personalIdentificationNumber, ct),
            token: cancellationToken);
    }

    private async Task<string?> GetNameFromRegister(string personalIdentificationNumber, CancellationToken cancellationToken)
    {
        const string apiUrl = "register/api/v1/parties/nameslookup";

        var nameLookup = new NameLookup
        {
            Parties = [
                new() { Ssn = personalIdentificationNumber }
            ]
        };

        var requestJson = JsonSerializer.Serialize(nameLookup, SerializerOptions);
        var httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync(apiUrl, httpContent, cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                nameof(NameRegistryClient) + ".SendRequest failed with non-successful status code: {StatusCode} {Response}",
                response.StatusCode, errorResponse);

            return null;
        }

        var responseData = await response.Content.ReadAsStringAsync(cancellationToken);
        var nameLookupResult = JsonSerializer.Deserialize<NameLookupResult>(responseData, SerializerOptions);
        return nameLookupResult?.PartyNames.FirstOrDefault()?.Name;
    }

    private sealed class NameLookup
    {
        public List<NameLookupSsn> Parties { get; set; } = null!;
    }

    private sealed class NameLookupResult
    {
        public List<NameLookupSsn> PartyNames { get; set; } = null!;
    }

    private sealed class NameLookupSsn
    {
        public string Ssn { get; set; } = null!;
        public string? Name { get; set; }
    }
}
