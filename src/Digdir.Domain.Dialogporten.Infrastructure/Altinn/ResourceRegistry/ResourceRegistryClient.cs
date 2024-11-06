using System.Runtime.CompilerServices;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Common;
using Digdir.Domain.Dialogporten.Domain.ResourcePolicyMetadata;
using ZiggyCreatures.Caching.Fusion;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.ResourceRegistry;

internal sealed class ResourceRegistryClient : IResourceRegistry
{
    private const string ServiceResourceInformationCacheKey = "ServiceResourceInformationCacheKey";
    private const string ResourceTypeGenericAccess = "GenericAccessResource";
    private const string ResourceTypeAltinnApp = "AltinnApp";
    private const string ResourceTypeCorrespondence = "CorrespondenceService";
    private const string ResourceRegistryResourceEndpoint = "resourceregistry/api/v1/resource/";

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
        var resources = await FetchServiceResourceInformation(cancellationToken);
        return resources
            .Where(x => x.OwnerOrgNumber == orgNumber)
            .ToList();
    }

    public async Task<ServiceResourceInformation?> GetResourceInformation(
        string serviceResourceId,
        CancellationToken cancellationToken)
    {
        var resources = await FetchServiceResourceInformation(cancellationToken);
        return resources
            .FirstOrDefault(x => x.ResourceId == serviceResourceId);
    }

    public async IAsyncEnumerable<List<UpdatedSubjectResource>> GetUpdatedSubjectResources(DateTimeOffset since, int batchSize, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string searchEndpoint = $"{ResourceRegistryResourceEndpoint}updated";
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

    public IAsyncEnumerable<List<ResourcePolicyMetadata>> GetUpdatedResourcePolicyMetadata(DateTimeOffset since, int batchSize,
        CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    private async Task<ServiceResourceInformation[]> FetchServiceResourceInformation(CancellationToken cancellationToken)
    {
        const string searchEndpoint = $"{ResourceRegistryResourceEndpoint}resourcelist";

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
