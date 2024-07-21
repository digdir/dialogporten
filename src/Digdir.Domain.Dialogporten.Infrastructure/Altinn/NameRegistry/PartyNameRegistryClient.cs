using System.Text.Json;
using System.Text.Json.Serialization;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Parties;
using ZiggyCreatures.Caching.Fusion;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.NameRegistry;

internal class PartyNameRegistryClient : IPartyNameRegistry
{
    private readonly IFusionCache _cache;
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    public PartyNameRegistryClient(HttpClient client, IFusionCacheProvider cacheProvider)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _cache = cacheProvider.GetCache(nameof(NameRegistry)) ?? throw new ArgumentNullException(nameof(cacheProvider));
    }

    public async Task<string?> GetName(string externalIdWithPrefix, CancellationToken cancellationToken)
    {
        return await _cache.GetOrSetAsync(
            $"Name_{externalIdWithPrefix}",
            ct => GetNameFromRegister(externalIdWithPrefix, ct),
            token: cancellationToken);
    }

    private async Task<string?> GetNameFromRegister(string externalIdWithPrefix, CancellationToken cancellationToken)
    {
        const string apiUrl = "register/api/v1/parties/nameslookup";

        var nameLookupParty = new NameLookupParty();
        if (NorwegianPersonIdentifier.TryParse(externalIdWithPrefix, out var personIdentifier))
        {
            nameLookupParty.Ssn = personIdentifier.Id;
        }
        else if (NorwegianOrganizationIdentifier.TryParse(externalIdWithPrefix, out var organizationIdentifier))
        {
            nameLookupParty.OrgNo = organizationIdentifier.Id;
        }
        else
        {
            return null;
        }

        var nameLookup = new NameLookup
        {
            Parties = [nameLookupParty]
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
        public List<NameLookupParty> Parties { get; set; } = null!;
    }

    private sealed class NameLookupResult
    {
        public List<NameLookupParty> PartyNames { get; set; } = null!;
    }

    private sealed class NameLookupParty
    {
        public string Ssn { get; set; } = null!;
        public string OrgNo { get; set; } = null!;
        public string? Name { get; set; }
    }
}
