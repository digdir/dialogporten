using System.Text.Json;
using System.Text.Json.Serialization;
using Digdir.Domain.Dialogporten.Application.Externals;
using ZiggyCreatures.Caching.Fusion;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.NameRegistry;

internal class NameRegistryClient : INameRegistry
{
    private readonly IFusionCache _cache;
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    public NameRegistryClient(HttpClient client, IFusionCacheProvider cacheProvider)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _cache = cacheProvider.GetCache(nameof(NameRegistry)) ?? throw new ArgumentNullException(nameof(cacheProvider));
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

        var nameLookupResult = await _client.PostAsJsonEnsuredAsync<NameLookupResult>(
            apiUrl,
            nameLookup,
            serializerOptions: SerializerOptions,
            cancellationToken: cancellationToken);

        return nameLookupResult.PartyNames.FirstOrDefault()?.Name;
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
