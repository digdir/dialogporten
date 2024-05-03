using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Common;
using ZiggyCreatures.Caching.Fusion;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.ResourceRegistry;

internal sealed class ResourceRegistryClient : IResourceRegistry
{
    private const string OrgResourceReferenceCacheKey = "OrgResourceReference";
    private const string ResourceTypeGenericAccess = "GenericAccessResource";
    private const string ResourceTypeAltinnApp = "AltinnApp";

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
        return resourceIds ?? [];
    }

    private async Task<Dictionary<string, string[]>> GetResourceIdsByOrg(CancellationToken cancellationToken)
    {
        const string searchEndpoint = "resourceregistry/api/v1/resource/resourcelist";

        var response = await _client
            .GetFromJsonEnsuredAsync<List<ResourceRegistryResponse>>(searchEndpoint,
                cancellationToken: cancellationToken);

        var resourceIdsByOrg = response
            .Where(x => x.ResourceType is ResourceTypeGenericAccess or ResourceTypeAltinnApp)
            .GroupBy(x => x.HasCompetentAuthority.Organization ?? string.Empty)
            .ToDictionary(
                x => x.Key,
                x => x.Select(
                    x => $"{Constants.ServiceResourcePrefix}{x.Identifier}")
                    .ToArray()
            );

        return resourceIdsByOrg;
    }

    private sealed class ResourceRegistryResponse
    {
        public required string Identifier { get; init; }
        public required CompetentAuthority HasCompetentAuthority { get; init; }
        public required string ResourceType { get; init; }
    }

    private sealed class CompetentAuthority
    {
        // Altinn 2 resources does not always have an organization number as competent authority, only service owner code
        // We filter these out anyway, but we need to allow null here
        public string? Organization { get; init; }
        public required string OrgCode { get; init; }
    }
}
