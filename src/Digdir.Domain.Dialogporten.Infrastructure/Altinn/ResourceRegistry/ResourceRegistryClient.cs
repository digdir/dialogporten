using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Common;
using ZiggyCreatures.Caching.Fusion;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.ResourceRegistry;

internal sealed class ResourceRegistryClient : IResourceRegistry
{
    private const string OrgResourceReferenceCacheKey = "OrgResourceReference";
    private const string ResourceTypeGenericAccess = "GenericAccessResource";
    private const string ResourceTypeAltinnApp = "AltinnApp";
    private const string ResourceTypeCorrespondence = "Correspondence";

    private readonly IFusionCache _cache;
    private readonly HttpClient _client;

    public ResourceRegistryClient(HttpClient client, IFusionCacheProvider cacheProvider)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _cache = cacheProvider.GetCache(nameof(ResourceRegistry)) ?? throw new ArgumentNullException(nameof(cacheProvider));
    }

    private async Task<Dictionary<string, AltinnResourceInformation[]>> GetResourceInfoByOrg(CancellationToken cancellationToken) =>
        await _cache.GetOrSetAsync(
            OrgResourceReferenceCacheKey,
            async token => await GetResourceInfoByOrgFromAltinn(token),
            token: cancellationToken);

    public async Task<IReadOnlyCollection<string>> GetResourceIds(string org, CancellationToken cancellationToken)
    {
        var resourceIdsByOrg = await GetResourceInfoByOrg(cancellationToken);
        resourceIdsByOrg.TryGetValue(org, out var resourceInfos);
        return resourceInfos?.Select(x => x.ResourceId).ToList() ?? [];
    }

    public async Task<string> GetResourceType(string orgNumber, string serviceResourceId, CancellationToken token)
    {
        var resourceIdsByOrg = await GetResourceInfoByOrg(token);
        resourceIdsByOrg.TryGetValue(orgNumber, out var resourceInfo);

        return resourceInfo?
            .FirstOrDefault(x => x.ResourceId == serviceResourceId)?
            .ResourceType ??
               throw new KeyNotFoundException();
    }

    private async Task<Dictionary<string, AltinnResourceInformation[]>> GetResourceInfoByOrgFromAltinn(CancellationToken cancellationToken)
    {
        const string searchEndpoint = "resourceregistry/api/v1/resource/resourcelist";

        var response = await _client
            .GetFromJsonEnsuredAsync<List<ResourceRegistryResponse>>(searchEndpoint,
                cancellationToken: cancellationToken);

        var resourceInfoByOrg = response
            .Where(x => !string.IsNullOrWhiteSpace(x.HasCompetentAuthority.Organization))
            .Where(x => x.ResourceType is
                ResourceTypeGenericAccess or
                ResourceTypeAltinnApp or
                ResourceTypeCorrespondence)
            .GroupBy(x => x.HasCompetentAuthority.Organization!)
            .ToDictionary(
                x => x.Key,
                x => x.Select(
                    x => new AltinnResourceInformation($"{Constants.ServiceResourcePrefix}{x.Identifier}", x.ResourceType))
                    .ToArray()
            );

        return resourceInfoByOrg;
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

public sealed record AltinnResourceInformation(string ResourceId, string ResourceType);
