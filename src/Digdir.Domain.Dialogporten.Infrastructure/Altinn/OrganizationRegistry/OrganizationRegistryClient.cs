using System.Diagnostics;
using System.Net.Http.Json;
using Digdir.Domain.Dialogporten.Application.Externals;
using Microsoft.Extensions.Caching.Distributed;
using ZiggyCreatures.Caching.Fusion;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.OrganizationRegistry;

internal class OrganizationRegistryClient : IOrganizationRegistry
{
    private const string OrgNameReferenceCacheKey = "OrgNameReference";

    private static readonly DistributedCacheEntryOptions OneDayCacheDuration = new() { AbsoluteExpiration = DateTimeOffset.UtcNow.AddDays(1) };
    private static readonly DistributedCacheEntryOptions ZeroCacheDuration = new() { AbsoluteExpiration = DateTimeOffset.MinValue };

    private readonly IFusionCache _cache;
    private readonly HttpClient _client;

    public OrganizationRegistryClient(HttpClient client, IFusionCacheProvider cacheProvider)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _cache = cacheProvider.GetCache(nameof(OrganizationRegistry)) ?? throw new ArgumentNullException(nameof(cacheProvider));
    }

    public async Task<OrganizationInfo?> GetOrgInfo(string orgNumber, CancellationToken cancellationToken)
    {
        var orgInfoByOrgNumber = await _cache.GetOrSetAsync(OrgNameReferenceCacheKey, async token => await GetOrgInfoByOrgNumber(token), token: cancellationToken);
        orgInfoByOrgNumber.TryGetValue(orgNumber, out var orgInfo);

        // todo: return org info: shortname and long names
        return orgInfo;
    }

    private async Task<Dictionary<string, OrganizationInfo>> GetOrgInfoByOrgNumber(CancellationToken cancellationToken)
    {
        const string searchEndpoint = "orgs/altinn-orgs.json";
        var response = await _client
            .GetFromJsonAsync<OrganizationRegistryResponse>(searchEndpoint, cancellationToken) ?? throw new UnreachableException();

        var orgLongNamesByOrgNumber = response
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

        return orgLongNamesByOrgNumber;
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