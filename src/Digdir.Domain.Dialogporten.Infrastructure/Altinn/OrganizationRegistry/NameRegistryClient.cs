using System.Globalization;
using Bogus;
using Digdir.Domain.Dialogporten.Application.Externals;
using Microsoft.Extensions.Caching.Distributed;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.OrganizationRegistry;

internal class NameRegistryClient : INameRegistry
{
    private readonly IDistributedCache _cache;
    private readonly HttpClient _client;

    public NameRegistryClient(HttpClient client, IDistributedCache cache)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<string?> GetName(string personalIdentificationNumber, CancellationToken cancellationToken)
    {
        // TODO: Implement fetching from Altinn
        // https://github.com/digdir/dialogporten/issues/321
        Randomizer.Seed = new Random((int)long.Parse(personalIdentificationNumber, new NumberFormatInfo()));
        var name = new Faker().Name.FullName();

        return await Task.FromResult(name);
    }
}
