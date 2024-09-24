using System.Runtime.CompilerServices;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Common;
using ZiggyCreatures.Caching.Fusion;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.ResourceRegistry;

internal sealed class ResourceRegistryClient : IResourceRegistry
{
    private const string ServiceResourceInformationByOrgCacheKey = "ServiceResourceInformationByOrgCacheKey";
    private const string ServiceResourceInformationByResourceIdCacheKey = "ServiceResourceInformationByResourceIdCacheKey";
    private const string ServiceResourceInformationCacheKey = "ServiceResourceInformationCacheKey";
    private const string ResourceTypeGenericAccess = "GenericAccessResource";
    private const string ResourceTypeAltinnApp = "AltinnApp";
    private const string ResourceTypeCorrespondence = "CorrespondenceService";

    private readonly IFusionCache _cache;
    private readonly HttpClient _client;

    public ResourceRegistryClient(HttpClient client, IFusionCacheProvider cacheProvider)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _cache = cacheProvider.GetCache(nameof(ResourceRegistry)) ?? throw new ArgumentNullException(nameof(cacheProvider));
    }

    public async Task<IReadOnlyCollection<ServiceResourceInformation>> GetResourceInformationForOrg(
        string orgNumber,
        CancellationToken cancellationToken)
    {
        var dic = await GetOrSetResourceInformationByOrg(cancellationToken);
        if (!dic.TryGetValue(orgNumber, out var resources))
        {
            resources = [];
        }

        return resources.AsReadOnly();
    }

    public async Task<ServiceResourceInformation?> GetResourceInformation(
        string serviceResourceId,
        CancellationToken cancellationToken)
    {
        var dic = await GetOrSetResourceInformationByResourceId(cancellationToken);
        dic.TryGetValue(serviceResourceId, out var resource);
        return resource;
    }

    public async IAsyncEnumerable<List<UpdatedSubjectResource>> GetUpdatedSubjectResources(DateTimeOffset since, int batchSize, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string searchEndpoint = "resourceregistry/api/v1/resource/updated";
        var nextUrl = searchEndpoint + $"?since={Uri.EscapeDataString(since.ToString("O"))}&limit={batchSize}";

        do
        {
            var response = await _client
                .GetFromJsonEnsuredAsync<UpdatedResponse>(nextUrl,
                    cancellationToken: cancellationToken);

            if (response.Data.Count == 0)
            {
                yield break;
            }

            yield return response.Data;

            nextUrl = response.Links.Next?.ToString();
        } while (nextUrl is not null);
    }


    private async Task<Dictionary<string, ServiceResourceInformation[]>> GetOrSetResourceInformationByOrg(
        CancellationToken cancellationToken)
    {
        var resources = await FetchServiceResourceInformation(cancellationToken);
        return resources
            .GroupBy(x => x.OwnerOrgNumber)
            .ToDictionary(x => x.Key, x => x.ToArray());
    }

    private async Task<Dictionary<string, ServiceResourceInformation>> GetOrSetResourceInformationByResourceId(
        CancellationToken cancellationToken)
    {
        var resources = await FetchServiceResourceInformation(cancellationToken);
        return resources.ToDictionary(x => x.ResourceId);
    }

    private async Task<ServiceResourceInformation[]> FetchServiceResourceInformation(CancellationToken cancellationToken)
    {
        const string searchEndpoint = "resourceregistry/api/v1/resource/resourcelist";

        return await _cache.GetOrSetAsync(
            ServiceResourceInformationCacheKey,
            async cToken =>
            {
                var response = await _client
                    .GetFromJsonEnsuredAsync<List<ResourceListResponse>>(searchEndpoint,
                        cancellationToken: cToken);

                return response
                    .Where(x => !string.IsNullOrWhiteSpace(x.HasCompetentAuthority.Organization))
                    .Where(x => x.ResourceType is
                        ResourceTypeGenericAccess or
                        ResourceTypeAltinnApp or
                        ResourceTypeCorrespondence)
                    .Select(x => new ServiceResourceInformation(
                        $"{Constants.ServiceResourcePrefix}{x.Identifier}",
                        x.ResourceType,
                        x.HasCompetentAuthority.Organization!))
                    .ToArray();
            },
            token: cancellationToken);
    }

    private sealed class ResourceListResponse
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

    private sealed record UpdatedResponse(UpdatedResponseLinks Links, List<UpdatedSubjectResource> Data);
    private sealed record UpdatedResponseLinks(Uri? Next);
}
