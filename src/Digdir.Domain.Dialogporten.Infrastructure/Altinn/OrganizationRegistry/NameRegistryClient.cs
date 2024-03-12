using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Infrastructure.Common.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.OrganizationRegistry;

internal class NameRegistryClient : INameRegistry
{
    private static readonly DistributedCacheEntryOptions _oneDayCacheDuration = new() { AbsoluteExpiration = DateTimeOffset.UtcNow.AddDays(1) };
    private static readonly DistributedCacheEntryOptions _zeroCacheDuration = new() { AbsoluteExpiration = DateTimeOffset.MinValue };

    private readonly IDistributedCache _cache;
    private readonly HttpClient _client;
    private readonly ILogger<NameRegistryClient> _logger;

    private static readonly JsonSerializerOptions _serializerOptions = new()

    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    public NameRegistryClient(HttpClient client, IDistributedCache cache, ILogger<NameRegistryClient> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string?> GetName(string personalIdentificationNumber, CancellationToken cancellationToken)
    {
        return await _cache.GetOrAddAsync(
            $"Name_{personalIdentificationNumber}",
            (ct) => GetNameFromRegister(personalIdentificationNumber, ct),
            CacheOptionsFactory,
            cancellationToken: cancellationToken);
    }

    private static DistributedCacheEntryOptions CacheOptionsFactory(string? name) =>
        name is not null ? _oneDayCacheDuration : _zeroCacheDuration;

    private async Task<string?> GetNameFromRegister(string personalIdentificationNumber, CancellationToken cancellationToken)
    {
        const string apiUrl = "register/api/v1/parties/nameslookup";

        var nameLookup = new NameLookup
        {
            Parties = [
                new() { Ssn = personalIdentificationNumber }
            ]
        };

        var requestJson = JsonSerializer.Serialize(nameLookup, _serializerOptions);
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
        var nameLookupResult = JsonSerializer.Deserialize<NameLookupResult>(responseData, _serializerOptions);
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
