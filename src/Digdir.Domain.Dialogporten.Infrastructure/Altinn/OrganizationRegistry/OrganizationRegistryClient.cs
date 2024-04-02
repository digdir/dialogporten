using System.Diagnostics;
using System.Net.Http.Json;
using Digdir.Domain.Dialogporten.Application.Externals;
using Microsoft.Extensions.Caching.Distributed;
using ZiggyCreatures.Caching.Fusion;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.OrganizationRegistry;

internal class OrganizationRegistryClient : IOrganizationRegistry
{
    private const string OrgShortNameReferenceCacheKey = "OrgShortNameReference";
    private static readonly DistributedCacheEntryOptions _oneDayCacheDuration = new() { AbsoluteExpiration = DateTimeOffset.UtcNow.AddDays(1) };
    private static readonly DistributedCacheEntryOptions _zeroCacheDuration = new() { AbsoluteExpiration = DateTimeOffset.MinValue };

    private readonly IFusionCache _cache;
    private readonly HttpClient _client;

    public OrganizationRegistryClient(HttpClient client, IFusionCacheProvider cacheProvider)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _cache = cacheProvider.GetCache(nameof(OrganizationRegistry)) ?? throw new ArgumentNullException(nameof(cacheProvider));
    }
    public async Task<string?> GetOrgShortName(string orgNumber, CancellationToken cancellationToken)
    {
        var orgShortNameByOrgNumber = await _cache.GetOrSetAsync(OrgShortNameReferenceCacheKey, async token => await GetOrgShortNameByOrgNumber(token), token: cancellationToken);
        orgShortNameByOrgNumber.TryGetValue(orgNumber, out var orgShortName);

        return orgShortName;
    }

    private async Task<Dictionary<string, string>> GetOrgShortNameByOrgNumber(CancellationToken cancellationToken)
    {
        const string searchEndpoint = "orgs/altinn-orgs.json";
        var response = await _client
            .GetFromJsonAsync<OrganizationRegistryResponse>(searchEndpoint, cancellationToken) ?? throw new UnreachableException();

        var orgShortNameByOrgNumber = response
            .Orgs
            .ToDictionary(
                pair => pair.Value.Orgnr,
                pair => pair.Key
            );

        return orgShortNameByOrgNumber;
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
