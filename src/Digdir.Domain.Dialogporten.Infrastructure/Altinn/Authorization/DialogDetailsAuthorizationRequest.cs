using System.Security.Claims;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.Authorization;

public sealed class DialogDetailsAuthorizationRequest
{
    public required ClaimsPrincipal ClaimsPrincipal { get; init; }
    public required string ServiceResource { get; init; }
    public required Guid DialogId { get; init; }
    public required string Party { get; init; }

    // Each action applies to a resource. This is the main resource and/or one or more dialog elements.
    public required Dictionary<string, List<string>> AuthorizationAttributesByActions { get; init; }
}
