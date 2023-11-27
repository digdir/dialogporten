using System.Security.Claims;

namespace Digdir.Domain.Dialogporten.Domain.Authorization;

public sealed class DialogDetailsAuthorizationRequest
{
    public ClaimsPrincipal ClaimsPrincipal { get; set; } = null!;
    public string ServiceResource { get; set; } = null!;
    public Guid DialogId { get; set; } = Guid.Empty;
    public string Party { get; set; } = null!;

    public List<string> Actions { get; set; } = new();

    // An authorization attribute is an additional resource attribute associated with an action
    // For dialog elements it is also an action, but implictly the action "elementread"
    public Dictionary<string, string> AuthorizationAttributes { get; set; } = new();
}
