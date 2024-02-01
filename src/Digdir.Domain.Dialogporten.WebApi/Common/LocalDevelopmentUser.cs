using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using System.Security.Claims;

namespace Digdir.Domain.Dialogporten.WebApi.Common;

internal sealed class LocalDevelopmentUser : IUser
{
    private readonly ClaimsPrincipal _principal = new(new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.Name, "Local Development User"),
        new Claim(ClaimTypes.NameIdentifier, "local-development-user"),
        new Claim("pid", "03886595947"),
        new Claim("scope", string.Join(" ", AuthorizationScope.AllScopes.Value)),
        new Claim("consumer",
            """
            {
                "authority": "iso6523-actorid-upis",
                "ID": "0192:991825827"
            }
            """)
    }));

    public ClaimsPrincipal GetPrincipal() => _principal;
}
