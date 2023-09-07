using Digdir.Domain.Dialogporten.Application.Externals;
using System.Net.Http.Json;

namespace Digdir.Domain.Dialogporten.Infrastructure;

internal sealed class ResourceRegistery : IResourceRegistry
{
    private readonly HttpClient _client;

    public ResourceRegistery(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<string?> GetOrgOwner(string resourceId, CancellationToken cancellationToken)
    {
        var response = await _client.GetAsync($"https://platform.tt02.altinn.no/resourceregistry/api/v1/resource/{resourceId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var responseContent = await response.Content.ReadFromJsonAsync<ResourceRegistryResponse>(cancellationToken: cancellationToken);

        return responseContent?.HasCompetentAuthority.Organization;
    }

    public Task<List<string>> GetResourceIds(string orgNumber, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private class ResourceRegistryResponse
    {
        public required CompetentAuthority HasCompetentAuthority { get; init; }
    }

    private class CompetentAuthority
    {
        public required string Organization { get; init; }
    }
}
