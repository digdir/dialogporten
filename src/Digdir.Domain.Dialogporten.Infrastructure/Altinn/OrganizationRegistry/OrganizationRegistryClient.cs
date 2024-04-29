using Digdir.Domain.Dialogporten.Application.Externals;
using ZiggyCreatures.Caching.Fusion;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.OrganizationRegistry;

internal class OrganizationRegistryClient : IOrganizationRegistry
{
    private const string OrgNameReferenceCacheKey = "OrgNameReference";

    private readonly IFusionCache _cache;
    private readonly HttpClient _client;

    public OrganizationRegistryClient(HttpClient client, IFusionCacheProvider cacheProvider)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _cache = cacheProvider.GetCache(nameof(OrganizationRegistry)) ?? throw new ArgumentNullException(nameof(cacheProvider));
    }

    public async Task<OrganizationInfo?> GetOrgInfo(string orgNumber, CancellationToken cancellationToken)
    {
        var orgInfoByOrgNumber = await _cache.GetOrSetAsync(OrgNameReferenceCacheKey, GetOrgInfo, token: cancellationToken);
        orgInfoByOrgNumber.TryGetValue(orgNumber, out var orgInfo);

        return orgInfo;
    }

    private async Task<Dictionary<string, OrganizationInfo>> GetOrgInfo(CancellationToken cancellationToken)
    {
        const string searchEndpoint = "orgs/altinn-orgs.json";

        var response = await _client
            .GetFromJsonEnsuredAsync<OrganizationRegistryResponse>(searchEndpoint, cancellationToken: cancellationToken);

        var orgInfoByOrgNumber = response
            .Orgs
            .ToDictionary(pair => pair.Value.Orgnr, pair => new OrganizationInfo
            {
                OrgNumber = pair.Value.Orgnr,
                ShortName = pair.Key,
                LongNames = pair.Value.Name?.Select(name => new OrganizationLongName
                {
                    LongName = name.Value,
                    Language = name.Key
                }).ToList() ?? new List<OrganizationLongName>()
            });

        return orgInfoByOrgNumber;
    }

    private sealed class OrganizationRegistryResponse
    {
        public required IDictionary<string, OrganizationDetails> Orgs { get; init; }
    }

    private sealed class OrganizationDetails
    {
        public IDictionary<string, string>? Name { get; init; }
        public string? Logo { get; init; }
        public required string Orgnr { get; init; }
        public string? Homepage { get; init; }
        public IList<string>? Environments { get; init; }
    }
}
