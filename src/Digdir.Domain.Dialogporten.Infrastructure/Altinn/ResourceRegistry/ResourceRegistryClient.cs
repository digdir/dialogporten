using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml;
using Altinn.Authorization.ABAC.Utils;
using Altinn.Authorization.ABAC.Xacml;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Common;
using Microsoft.Extensions.Logging;
using ZiggyCreatures.Caching.Fusion;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.ResourceRegistry;

internal sealed class ResourceRegistryClient : IResourceRegistry
{
    private const string ServiceResourceInformationCacheKey = "ServiceResourceInformationCacheKey";
    private const string ResourceTypeGenericAccess = "GenericAccessResource";
    private const string ResourceTypeAltinnApp = "AltinnApp";
    private const string ResourceTypeCorrespondence = "CorrespondenceService";
    private const string ResourceRegistryResourceEndpoint = "resourceregistry/api/v1/resource/";

    private const int DefaultMinimumSecurityLevel = 1;
    private const string AuthenticationLevelCategory = "urn:altinn:minimum-authenticationlevel";

    private readonly IFusionCache _cache;
    private readonly HttpClient _client;
    private readonly ILogger<ResourceRegistryClient> _logger;

    public ResourceRegistryClient(HttpClient client, IFusionCacheProvider cacheProvider, ILogger<ResourceRegistryClient> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _cache = cacheProvider.GetCache(nameof(ResourceRegistry)) ?? throw new ArgumentNullException(nameof(cacheProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

    public async Task<IReadOnlyCollection<UpdatedResourcePolicyInformation>> GetUpdatedResourcePolicyInformation(DateTimeOffset since,
        int numberOfConcurrentRequests,
        CancellationToken cancellationToken)
    {
        // First fetch all unique updated resources serially using the supplied "since" value (but a fixed batch size; 1000 seems to be a good value)
        // We then iterate them, and only keep the latest updated resource for each unique resource URN. We then fan out to fetch and parse the policies
        // concurrently using semaphores.

        var updatedResources = await GetUniqueUpdatedResources(since, cancellationToken);

        var semaphore = new SemaphoreSlim(numberOfConcurrentRequests);
        var metadataTasks = new List<Task<UpdatedResourcePolicyInformation>>();

        foreach (var updatedResource in updatedResources)
        {
            await semaphore.WaitAsync(cancellationToken);
            var task = Task.Run(async () =>
            {
                try
                {
                    return await GetUpdatedResourcePolicyInformation(updatedResource, cancellationToken);
                }
                finally
                {
                    semaphore.Release();
                }
            }, cancellationToken);

            metadataTasks.Add(task);
        }

        return await Task.WhenAll(metadataTasks);
    }

    private async Task<List<UpdatedResource>> GetUniqueUpdatedResources(DateTimeOffset since, CancellationToken cancellationToken)
    {
        var updatedResources = new Dictionary<Uri, UpdatedSubjectResource>();
        await foreach (var subjectResources in GetUpdatedSubjectResources(since, 1000, cancellationToken))
        {
            foreach (var subjectResource in subjectResources)
            {
                if (updatedResources.TryGetValue(subjectResource.ResourceUrn, out var value) && value.UpdatedAt > subjectResource.UpdatedAt)
                {
                    continue;
                }

                updatedResources[subjectResource.ResourceUrn] = subjectResource;
            }
        }

        return updatedResources
            .Select(x => new UpdatedResource(x.Value.ResourceUrn, x.Value.UpdatedAt))
            .ToList();
    }

    private async Task<UpdatedResourcePolicyInformation> GetUpdatedResourcePolicyInformation(UpdatedResource resource, CancellationToken cancellationToken)
    {
        var resourceRegistryEntry = new ResourceRegistryEntry(resource.ResourceUrn);
        if (!resourceRegistryEntry.HasPolicyInResourceRegistry)
        {
            return new UpdatedResourcePolicyInformation(resource.ResourceUrn, DefaultMinimumSecurityLevel,
                resource.UpdatedAt);
        }

        try
        {
            var policy = await FetchPolicy(resourceRegistryEntry.Identifier, cancellationToken);
            return new UpdatedResourcePolicyInformation(
                resource.ResourceUrn,
                GetMinimumSecurityLevel(policy),
                resource.UpdatedAt);
        }
        catch (Exception ex)
        {
            // We need to keep going here, so we log and return a default value
            _logger.LogError("Failed to process policy for \"{ResourceUrn}\": {Exception}", resource.ResourceUrn, ex);
            return new UpdatedResourcePolicyInformation(resource.ResourceUrn, DefaultMinimumSecurityLevel,
                resource.UpdatedAt);
        }
    }

    private async Task<XacmlPolicy> FetchPolicy(string resourceIdentifier, CancellationToken cancellationToken)
    {
        const string policyEndpointPattern = ResourceRegistryResourceEndpoint + "{0}/policy";

        var policyXml = await _client
            .GetStreamAsync(string.Format(CultureInfo.InvariantCulture, policyEndpointPattern, resourceIdentifier),
                cancellationToken: cancellationToken);

        var policy = XacmlParser.ParseXacmlPolicy(XmlReader.Create(policyXml))
                     ?? throw new InvalidOperationException($"Failed to parse XACML policy for \"{resourceIdentifier}\".");

        return policy;
    }

    private static int GetMinimumSecurityLevel(XacmlPolicy policy)
    {
        var authenticationLevelAttributeAssigmentExpression = policy.ObligationExpressions
            .FirstOrDefault()?.AttributeAssignmentExpressions
            .FirstOrDefault(y => y.Category.ToString() == AuthenticationLevelCategory);

        return authenticationLevelAttributeAssigmentExpression is null
            ? DefaultMinimumSecurityLevel
            : int.TryParse(((XacmlAttributeValue)authenticationLevelAttributeAssigmentExpression.Property).Value, out var minimumSecurityLevel)
                ? minimumSecurityLevel
                : DefaultMinimumSecurityLevel;
    }

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

    private sealed class ResourceRegistryEntry
    {
        public string Identifier { get; }
        public bool HasPolicyInResourceRegistry { get; }

        private const string Altinn2ServicePrefix = "se_";
        private const string AltinnAppPrefix = "app_";
        private const char UrnSeparator = ':';

        public ResourceRegistryEntry(Uri resourceUrn)
        {
            // Utility class to extract the identifier from a resource URN, and determine if it has a policy
            // available in the resource registry API (Altinn 2 representations and Altinn Apps do not)
            var fullIdentifier = resourceUrn.ToString();
            Identifier = fullIdentifier[(fullIdentifier.LastIndexOf(UrnSeparator) + 1)..];
            HasPolicyInResourceRegistry = !Identifier.StartsWith(Altinn2ServicePrefix, StringComparison.Ordinal)
                        && !Identifier.StartsWith(AltinnAppPrefix, StringComparison.Ordinal);
        }
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
