namespace Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;

public sealed class DialogSearchAuthorizationResult
{
    // Resources here are "main" resources, eg. something that represents an entry in the Resource Registry
    // eg. "urn:altinn:resource:some-service" and referred to by "ServiceResource" in DialogEntity
    public Dictionary<string, List<string>> ResourcesByParties { get; init; } = new();
    public Dictionary<string, List<string>> PartiesByResources { get; init; } = new();
    public List<Guid> DialogIds { get; init; } = [];

    public bool HasNoAuthorizations =>
        ResourcesByParties.Count == 0
        && PartiesByResources.Count == 0
        && DialogIds.Count == 0;
}
