using System.Security.Claims;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;

namespace Digdir.Domain.Dialogporten.Application.Integration.Tests.Common;

internal sealed class IntegrationTestUser : IUser
{
    private readonly ClaimsPrincipal _principal = new(new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.Name, "Integration Test User"),
        new Claim(ClaimTypes.NameIdentifier, "integration-test-user"),
        new Claim("pid", "22834498646"),
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
