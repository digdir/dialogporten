using System.Security.Claims;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.Authorization;

public sealed class DialogSearchAuthorizationRequest
{
    public required List<Claim> Claims { get; init; }
    public List<string> ConstraintParties { get; set; } = [];
    public List<string> ConstraintServiceResources { get; set; } = [];
}
