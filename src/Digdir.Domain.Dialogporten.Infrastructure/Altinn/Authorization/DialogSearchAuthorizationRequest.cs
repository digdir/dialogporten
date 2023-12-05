using System.Security.Claims;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.Authorization;

public sealed class DialogSearchAuthorizationRequest
{
    public ClaimsPrincipal ClaimsPrincipal { get; set; } = null!;
    public List<string>? ConstraintParties { get; set; }
    public List<string>? ConstraintServiceResources { get; set; }
}
