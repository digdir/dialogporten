namespace Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;

public sealed class DialogSearchAuthorizationResult
{
    public Dictionary<string, List<string>> ResourcesByParties { get; init; } = new();
    public Dictionary<string, List<string>> PartiesByResources { get; init; } = new();
    public List<Guid> DialogIds { get; init; } = new();
}
