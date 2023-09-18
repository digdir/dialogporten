using Digdir.Domain.Dialogporten.Application.Externals;
using Polly;
using Polly.Registry;
using System.Net.Http.Json;

namespace Digdir.Domain.Dialogporten.Infrastructure;

internal sealed class ResourceRegistery : IResourceRegistry
{
    private readonly ResourceRegistryClient _client;
    private readonly IAsyncPolicy<OrgResourceReference> _cachePolicy;

    public ResourceRegistery(ResourceRegistryClient client, IReadOnlyPolicyRegistry<string> policyRegistry)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _cachePolicy = policyRegistry.Get<IAsyncPolicy<OrgResourceReference>>("OrgResourceReferenceCache");
    }

    public async Task<string?> GetOrgOwner(string resourceId, CancellationToken cancellationToken)
    {
        var reference = await GetOrgResourceReference(cancellationToken);
        reference.OrgByResourceId.TryGetValue(resourceId, out var owner);
        return owner;
    }

    public async Task<List<string>> GetResourceIds(string org, CancellationToken cancellationToken)
    {
        var reference = await GetOrgResourceReference(cancellationToken);
        reference.ResourceIdsByOrg.TryGetValue(org, out var resourceIds);
        return resourceIds ?? new List<string>();
    }

    private async Task<OrgResourceReference> GetOrgResourceReference(CancellationToken cancellationToken)
    {
        return await _cachePolicy.ExecuteAsync((ctx, ct) => _client.GetOrgResourceReference(ct), new Context("ThisIsMyCacheKey"), cancellationToken);
    }
}


internal sealed class ResourceRegistryClient
{
    private readonly HttpClient _client;

    public ResourceRegistryClient(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<OrgResourceReference> GetOrgResourceReference(CancellationToken cancellationToken)
    {
        const string SearchEndpoint = "resourceregistry/api/v1/resource/search";
        var response = await _client.GetAsync(SearchEndpoint, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            // TODO: What to do? 
            throw new Exception();
        }

        var responseContent = await response.Content.ReadFromJsonAsync<List<ResourceRegistryResponse>>(cancellationToken: cancellationToken);

        if (responseContent is null)
        {
            // TODO: What to do? 
            throw new Exception();
        }

        var reference = new OrgResourceReference(
            responseContent
                .ToDictionary(x => x.Identifier, x => x.HasCompetentAuthority.Organization),
            responseContent
                .GroupBy(x => x.HasCompetentAuthority.Organization)
                .ToDictionary(x => x.Key, x => x.Select(x => x.Identifier).ToList()));

        return reference;
    }

    private class ResourceRegistryResponse
    {
        public required string Identifier { get; init; }
        public required CompetentAuthority HasCompetentAuthority { get; init; }
    }

    private class CompetentAuthority
    {
        public required string Organization { get; init; }
    }
}

internal sealed record OrgResourceReference(
    Dictionary<string, string> OrgByResourceId,
    Dictionary<string, List<string>> ResourceIdsByOrg);