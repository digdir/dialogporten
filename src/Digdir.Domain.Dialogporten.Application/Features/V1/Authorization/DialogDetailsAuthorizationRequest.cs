using System.Security.Claims;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Authorization;

public sealed class DialogDetailsAuthorizationRequest
{


    public ClaimsPrincipal ClaimsPrincipal { get; set; } = null!;
    public string ServiceResource { get; set; } = null!;
    public Guid DialogId { get; set; } = Guid.Empty;
    public string Party { get; set; } = null!;

    // Each action applies to a resource. This is the main resource and/or one or more dialog elements.
    public Dictionary<string, List<string>> Actions { get; set; } = new();
}
