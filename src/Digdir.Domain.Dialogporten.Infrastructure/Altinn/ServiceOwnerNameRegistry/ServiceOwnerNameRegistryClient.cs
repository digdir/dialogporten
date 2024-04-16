using System.Diagnostics;
using System.Net.Http.Json;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Externals;
using ZiggyCreatures.Caching.Fusion;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.ServiceOwnerNameRegistry;

internal class ServiceOwnerNameRegistryClient : IServiceOwnerNameRegistry
{
    private const string ServiceOwnerShortNameReferenceCacheKey = "ServiceOwnerShortNameReference";

    private readonly IFusionCache _cache;
    private readonly HttpClient _client;

    public ServiceOwnerNameRegistryClient(HttpClient client, IFusionCacheProvider cacheProvider)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _cache = cacheProvider.GetCache(nameof(ServiceOwnerNameRegistry)) ?? throw new ArgumentNullException(nameof(cacheProvider));
    }
    public async Task<string?> GetServiceOwnerShortName(string orgNumber, CancellationToken cancellationToken)
    {
        var orgShortNameByOrgNumber = await _cache.GetOrSetAsync(ServiceOwnerShortNameReferenceCacheKey, async token => await GetOrgShortNameByOrgNumber(token), token: cancellationToken);
        orgShortNameByOrgNumber.TryGetValue(orgNumber, out var orgShortName);

        return orgShortName;
    }

    public async Task<List<LocalizedName>> GetServiceOwnerLongNames(string orgNumber, CancellationToken cancellationToken)
    {
        return await Task.FromResult(new List<LocalizedName>() { new() { Name = "Altinn" } });
    }

    private async Task<Dictionary<string, string>> GetOrgShortNameByOrgNumber(CancellationToken cancellationToken)
    {
        const string searchEndpoint = "orgs/altinn-orgs.json";
        var response = await _client
            .GetFromJsonAsync<ServiceOwnerRegistryResponse>(searchEndpoint, cancellationToken) ?? throw new UnreachableException();

        var orgShortNameByOrgNumber = response
            .Orgs
            .ToDictionary(
                pair => pair.Value.Orgnr,
                pair => pair.Key
            );

        return orgShortNameByOrgNumber;
    }

    private sealed class ServiceOwnerRegistryResponse
    {
        public required IDictionary<string, ServiceOwnerDetails> Orgs { get; init; }
    }

    private sealed class ServiceOwnerDetails
    {
        public IDictionary<string, string>? Name { get; init; }
        public string? Logo { get; init; }
        public required string Orgnr { get; init; }
        public string? Homepage { get; init; }
        public IList<string>? Environments { get; init; }
    }
}
