using BenchmarkDotNet.Attributes;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Parties;

namespace Digdir.Tool.Dialogporten.Benchmarks;

[MemoryDiagnoser]
public class QueryableExtensionsBenchmark
{
    private static readonly IQueryable<DialogEntity> _queryable = Enumerable.Empty<DialogEntity>().AsQueryable();
    private DialogSearchAuthorizationResult _authResult = null!;

    [Params(1, 10, 20)]
    public int Outer { get; set; }

    [Params(10, 100, 1000)]
    public int Inner { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _authResult = new()
        {
            DialogIds = Enumerable.Range(0, Outer)
                .Select(x => Guid.NewGuid())
                .ToList(),
            PartiesByResources = Enumerable.Range(0, Outer)
                .ToDictionary(
                    keySelector: GenerateResource,
                    elementSelector: outer => Enumerable.Range(0, Inner)
                        .Select(GenerateParty)
                        .ToList()
                ),
            ResourcesByParties = Enumerable.Range(0, Outer)
                .ToDictionary(
                    keySelector: GenerateParty,
                    elementSelector: outer => Enumerable.Range(0, Inner)
                        .Select(GenerateResource)
                        .ToList()
                )
        };
    }

    private static string GenerateParty(int count) => $"{NorwegianOrganizationIdentifier.Prefix}{999999999 - count}";
    private static string GenerateResource(int count) => $"urn:foo:bar{count}";

    [Benchmark]
    public void WhereUserIsAuthorizedFor()
    {
        _queryable.WhereUserIsAuthorizedFor(_authResult);
    }
}
