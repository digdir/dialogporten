using System.Security.Claims;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.Authorization;

public sealed class DialogSearchAuthorizationRequest
{
    public required List<Claim> Claims { get; init; }
    public List<string> ConstraintParties { get; set; } = new();
    public List<string> ConstraintServiceResources { get; set; } = new();
}
