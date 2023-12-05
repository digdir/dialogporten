using System.Security.Claims;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.Authorization;

public sealed class DialogDetailsAuthorizationRequest
{
    public required ClaimsPrincipal ClaimsPrincipal { get; init; }
    public required string ServiceResource { get; init; }
    public required Guid DialogId { get; init; }
    public required string Party { get; init; }

    // Each action applies to a resource. This is the main resource, or another resource indicated by a authorization attribute
    // eg. "urn:altinn:subresource:some-sub-resource" or "urn:altinn:task:task_1"
    public required Dictionary<string, List<string>> AuthorizationAttributesByActions { get; init; }
}
