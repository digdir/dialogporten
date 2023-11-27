namespace Digdir.Domain.Dialogporten.Domain.Authorization;

public sealed class DialogSearchAuthorizationResponse
{
    public Dictionary<string, List<string>> ResourcesForParties { get; set; } = new();
    public Dictionary<string, List<string>> PartiesForResources { get; set; } = new();
    public List<Guid> DialogIds { get; set; } = new();
}
