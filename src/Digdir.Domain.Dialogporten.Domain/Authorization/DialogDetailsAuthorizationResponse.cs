namespace Digdir.Domain.Dialogporten.Domain.Authorization;

public sealed class DialogDetailsAuthorizationResponse
{
    public List<string> AuthorizedActions { get; set; } = new();
    public Dictionary<string, string> AuthorizedAuthorizationAttributes { get; set; } = new();
}
