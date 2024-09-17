using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Parties;
using Digdir.Domain.Dialogporten.Domain.Parties.Abstractions;
using ZiggyCreatures.Caching.Fusion;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.NameRegistry;

internal sealed class PartyNameRegistryClient : IPartyNameRegistry
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

        if (!TryParse(externalIdWithPrefix, out var nameLookup))
        {
            return null;
        }

        var nameLookupResult = await _client.PostAsJsonEnsuredAsync<NameLookupResult>(
            apiUrl,
            nameLookup,
            serializerOptions: SerializerOptions,
            cancellationToken: cancellationToken);

        return nameLookupResult.PartyNames.FirstOrDefault()?.Name;
    }

    private static bool TryParse(string externalIdWithPrefix, [NotNullWhen(true)] out NameLookup? nameLookup)
    {
        if (!PartyIdentifier.TryParse(externalIdWithPrefix, out var partyIdentifier))
        {
            nameLookup = null;
            return false;
        }

        nameLookup = partyIdentifier switch
        {
            NorwegianPersonIdentifier personIdentifier => new() { Parties = [new() { Ssn = personIdentifier.Id }] },
            NorwegianOrganizationIdentifier organizationIdentifier => new() { Parties = [new() { OrgNo = organizationIdentifier.Id }] },
            _ => null
        };

        return nameLookup is not null;
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
