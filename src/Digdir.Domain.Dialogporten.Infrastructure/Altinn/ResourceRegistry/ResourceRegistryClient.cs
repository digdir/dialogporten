using System.Diagnostics;
using System.Net.Http.Json;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Common;
using Microsoft.Extensions.Caching.Distributed;
using ZiggyCreatures.Caching.Fusion;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.ResourceRegistry;

internal sealed class ResourceRegistryClient : IResourceRegistry
{
    private const string OrgResourceReferenceCacheKey = "OrgResourceReference";
    private static readonly DistributedCacheEntryOptions _oneDayCacheDuration = new() { AbsoluteExpiration = DateTimeOffset.UtcNow.AddDays(1) };
    private static readonly DistributedCacheEntryOptions _zeroCacheDuration = new() { AbsoluteExpiration = DateTimeOffset.MinValue };

    private readonly IFusionCache _cache;
    private readonly HttpClient _client;

    public ResourceRegistryClient(HttpClient client, IFusionCacheProvider cacheProvider)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _cache = cacheProvider.GetCache(nameof(ResourceRegistry)) ?? throw new ArgumentNullException(nameof(cacheProvider));
    }

    public async Task<IReadOnlyCollection<string>> GetResourceIds(string org, CancellationToken cancellationToken)
    {
        var resourceIdsByOrg = await _cache.GetOrSetAsync(
            OrgResourceReferenceCacheKey,
            async token => await GetResourceIdsByOrg(token),
            token: cancellationToken);
        resourceIdsByOrg.TryGetValue(org, out var resourceIds);
        return resourceIds ?? Array.Empty<string>();
    }

    private static DistributedCacheEntryOptions? CacheOptionsFactory(Dictionary<string, string[]>? resourceIdsByOrg) =>
        resourceIdsByOrg is not null ? _oneDayCacheDuration : _zeroCacheDuration;

    private async Task<Dictionary<string, string[]>> GetResourceIdsByOrg(CancellationToken cancellationToken)
    {
        const string searchEndpoint = "resourceregistry/api/v1/resource/search";
        var response = await _client
            .GetFromJsonAsync<List<ResourceRegistryResponse>>(searchEndpoint, cancellationToken)
            ?? throw new UnreachableException();

        var resourceIdsByOrg = response
            .GroupBy(x => x.HasCompetentAuthority.Organization)
            .ToDictionary(
                x => x.Key,
                x => x.Select(x => $"{Constants.ServiceResourcePrefix}{x.Identifier}")
                    .ToArray()
            );

        return resourceIdsByOrg;
    }

    private sealed class ResourceRegistryResponse
    {
        public required string Identifier { get; init; }
        public required CompetentAuthority HasCompetentAuthority { get; init; }
    }

    private sealed class CompetentAuthority
    {
        public required string Organization { get; init; }
    }
}
