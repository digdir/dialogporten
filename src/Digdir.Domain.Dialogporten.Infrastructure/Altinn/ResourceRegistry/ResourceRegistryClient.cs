using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml;
using Altinn.Authorization.ABAC.Utils;
using Altinn.Authorization.ABAC.Xacml;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Common;
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

    public async Task<IReadOnlyCollection<UpdatedResourcePolicyMetadata>> GetUpdatedResourcePolicyMetadata(DateTimeOffset since,
        int numberOfConcurrentRequests,
        CancellationToken cancellationToken)
    {
        var metadata = new List<UpdatedResourcePolicyMetadata>();

        // First fetch all unique updated resources using the same "since" value (but a custom batch size)
        var updatedResources = new Dictionary<Uri, UpdatedSubjectResource>();
        await foreach (var subjectResources in GetUpdatedSubjectResources(since, 1000, cancellationToken))
        {
            // Foreach of the unique resources referred to in this batch, fetch the policies
            foreach (var subjectResource in subjectResources)
            {
                updatedResources[subjectResource.ResourceUrn] = subjectResource;
            }
        }

        // Fan out to numberOfConcurrentRequests tasks to fetch and parse the policies concurrently
        var semaphore = new SemaphoreSlim(numberOfConcurrentRequests);
        var metadataTasks = new List<Task<UpdatedResourcePolicyMetadata>>();

        foreach (var updatedResource in updatedResources)
        {
            await semaphore.WaitAsync(cancellationToken);
            var task = Task.Run(async () =>
            {
                try
                {
                    return await GetUpdatedResourcePolicyMetadata(updatedResource.Value, cancellationToken);
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

    private async Task<UpdatedResourcePolicyMetadata> GetUpdatedResourcePolicyMetadata(UpdatedSubjectResource resource, CancellationToken cancellationToken)
    {
        var resourceRegistryEntry = new ResourceRegistryEntry(resource.ResourceUrn);
        if (!resourceRegistryEntry.HasPolicy)
        {
            // Altinn 2 and Altinn Apps does not have policies in the resource registry as of now, so we fake these
            return new UpdatedResourcePolicyMetadata(resource.ResourceUrn, DefaultMinimumSecurityLevel,
                resource.UpdatedAt);
        }

        try
        {
            var policy = await FetchPolicy(resourceRegistryEntry.Identifier, cancellationToken);
            return new UpdatedResourcePolicyMetadata(resource.ResourceUrn, GetMinimumSecurityLevel(policy),
                resource.UpdatedAt);
        }
        catch (Exception)
        {
            Console.Error.WriteLine($"Janitor: Failed to fetch policy for \"{resource.ResourceUrn}\".");
            return new UpdatedResourcePolicyMetadata(resource.ResourceUrn, DefaultMinimumSecurityLevel,
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
        var obligationExpression = policy.ObligationExpressions.FirstOrDefault();
        if (obligationExpression is null)
        {
            return DefaultMinimumSecurityLevel;
        }
        var authenticationLevelAttributeAssigmentExpression = obligationExpression.AttributeAssignmentExpressions
            .FirstOrDefault(y => y.Category.ToString() == AuthenticationLevelCategory);

        if (authenticationLevelAttributeAssigmentExpression is null)
        {
            return DefaultMinimumSecurityLevel;
        }

        return int.TryParse(((XacmlAttributeValue)authenticationLevelAttributeAssigmentExpression.Property).Value, out var minimumSecurityLevel)
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
        public string Identifier { get; init; }
        public bool HasPolicy { get; init; }

        public ResourceRegistryEntry(Uri resourceUrn)
        {
            var fullIdentifier = resourceUrn.ToString();
            Identifier = fullIdentifier[(fullIdentifier.LastIndexOf(':') + 1)..];
            HasPolicy = !Identifier.StartsWith("se_", StringComparison.Ordinal)
                        && !Identifier.StartsWith("app_", StringComparison.Ordinal);
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
