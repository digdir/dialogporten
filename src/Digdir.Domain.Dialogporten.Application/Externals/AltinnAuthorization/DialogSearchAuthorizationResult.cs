namespace Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;

public sealed class DialogSearchAuthorizationResult
{
    public Dictionary<string, List<string>> ResourcesForParties { get; set; } = new();
    public Dictionary<string, List<string>> PartiesForResources { get; set; } = new();
    public List<Guid> DialogIds { get; set; } = new();
}
