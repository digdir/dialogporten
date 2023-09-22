using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Infrastructure.Common.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using System.Diagnostics;
using System.Net.Http.Json;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.Registry;

internal sealed class ResourceRegistryClient : IResourceRegistry
{
    private const string OrgResourceReferenceCacheKey = "OrgResourceReference";
    private static readonly DistributedCacheEntryOptions _oneDayCacheDuration = new() { AbsoluteExpiration = DateTimeOffset.UtcNow.AddDays(1) };
    private static readonly DistributedCacheEntryOptions _zeroCacheDuration = new() { AbsoluteExpiration = DateTimeOffset.MinValue };

    private readonly IDistributedCache _cache;
    private readonly HttpClient _client;

    public ResourceRegistryClient(HttpClient client, IDistributedCache cache)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<string?> GetOrgOwner(string resourceId, CancellationToken cancellationToken)
    {
        var reference = await _cache.GetOrAddAsync(
            OrgResourceReferenceCacheKey,
            GetOrgResourceReference,
            CacheOptionsFactory,
            cancellationToken: cancellationToken);
        reference.OrgByResourceId.TryGetValue(resourceId, out var owner);
        return owner;
    }

    public async Task<string[]> GetResourceIds(string org, CancellationToken cancellationToken)
    {
        var reference = await _cache.GetOrAddAsync(
            OrgResourceReferenceCacheKey,
            GetOrgResourceReference,
            CacheOptionsFactory,
            cancellationToken: cancellationToken);
        reference.ResourceIdsByOrg.TryGetValue(org, out var resourceIds);
        return resourceIds ?? Array.Empty<string>();
    }

    private static DistributedCacheEntryOptions? CacheOptionsFactory(OrgResourceReference reference) => 
        reference is not null ? _oneDayCacheDuration : _zeroCacheDuration;

    private async Task<OrgResourceReference> GetOrgResourceReference(CancellationToken cancellationToken)
    {
        const string SearchEndpoint = "resourceregistry/api/v1/resource/search";
        var response = await _client.GetFromJsonAsync<List<ResourceRegistryResponse>>(SearchEndpoint, cancellationToken);

        if (response is null)
        {
            throw new UnreachableException();
        }

        var orgByResourceId = response
            .ToDictionary(x => x.Identifier, x => x.HasCompetentAuthority.Organization);
        var resourceIdsByOrg = response
            .GroupBy(x => x.HasCompetentAuthority.Organization)
            .ToDictionary(x => x.Key, x => x.Select(x => x.Identifier).ToArray());

        return new(orgByResourceId, resourceIdsByOrg);
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

    private sealed record OrgResourceReference(
        Dictionary<string, string> OrgByResourceId,
        Dictionary<string, string[]> ResourceIdsByOrg);
}
