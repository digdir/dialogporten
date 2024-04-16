using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Digdir.Domain.Dialogporten.Application.Externals;
using Microsoft.Extensions.Logging;
using ZiggyCreatures.Caching.Fusion;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.PartyNameRegistry;

internal class PartyNameRegistryClient : IPartyNameRegistry
{
    private readonly IFusionCache _cache;
    private readonly HttpClient _client;
    private readonly ILogger<PartyNameRegistryClient> _logger;

    private static readonly JsonSerializerOptions SerializerOptions = new()

    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    public PartyNameRegistryClient(HttpClient client, IFusionCacheProvider cacheProvider, ILogger<PartyNameRegistryClient> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _cache = cacheProvider.GetCache(nameof(PartyNameRegistry)) ?? throw new ArgumentNullException(nameof(cacheProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string?> GetPersonName(string personalIdentificationNumber, CancellationToken cancellationToken)
    {
        return await _cache.GetOrSetAsync(
            $"PersonName_{personalIdentificationNumber}",
            ct => GetNameFromRegister(PartyType.Person, personalIdentificationNumber, ct),
            token: cancellationToken);
    }

    public async Task<string?> GetOrganizationName(string organizationNumber, CancellationToken cancellationToken)
    {
        return await _cache.GetOrSetAsync(
            $"OrgName_{organizationNumber}",
            ct => GetNameFromRegister(PartyType.Organization, organizationNumber, ct),
            token: cancellationToken);
    }

    private async Task<string?> GetNameFromRegister(PartyType partyType, string identifier, CancellationToken cancellationToken)
    {
        const string apiUrl = "register/api/v1/parties/nameslookup";

        var nameLookup = new NameLookupRequest
        {
            Parties = [
                PartyType.Person == partyType
                    ? new PartyNameLookup() { Ssn = identifier }
                    : new PartyNameLookup() { OrgNo = identifier }
            ]
        };

        var requestJson = JsonSerializer.Serialize(nameLookup, SerializerOptions);
        var httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync(apiUrl, httpContent, cancellationToken);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                nameof(PartyNameRegistryClient) + ".SendRequest failed with non-successful status code: {StatusCode} {Response}",
                response.StatusCode, errorResponse);

            return null;
        }

        var responseData = await response.Content.ReadAsStringAsync(cancellationToken);
        var nameLookupResult = JsonSerializer.Deserialize<NameLookupResult>(responseData, SerializerOptions);
        return nameLookupResult?.PartyNames.FirstOrDefault()?.Name;
    }

    private sealed class NameLookupRequest
    {
        public List<PartyNameLookup> Parties { get; set; } = null!;
    }

    private sealed class NameLookupResult
    {
        public List<PartyNameLookup> PartyNames { get; set; } = null!;
    }

    private sealed class PartyNameLookup
    {
        public string? Ssn { get; set; }
        public string? OrgNo { get; set; }
        public string? Name { get; set; }
    }

    private enum PartyType
    {
        Person,
        Organization
    }
}
