using System.Security.Claims;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;

namespace Digdir.Domain.Dialogporten.Application.Unit.Tests.Features.V1.Common.Extensions;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void TryGetAuthenticationLevel_Should_Parse_Idporten_Acr_Claim_For_Level3()
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("acr", "idporten-loa-substantial")
        }));

        // Act
        var result = claimsPrincipal.TryGetAuthenticationLevel(out var authenticationLevel);

        // Assert
        Assert.True(result);
        Assert.Equal(3, authenticationLevel);
    }

    [Fact]
    public void TryGetAuthenticationLevel_Should_Parse_Idporten_Acr_Claim_For_Level4()
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("acr", "idporten-loa-high")
        }));

        // Act
        var result = claimsPrincipal.TryGetAuthenticationLevel(out var authenticationLevel);

        // Assert
        Assert.True(result);
        Assert.Equal(4, authenticationLevel);
    }

    [Fact]
    public void TryGetAuthenticationLevel_Should_Parse_Altinn_Authlevel_First()
    {
        // Arrange
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("acr", "idporten-loa-high"),
            new Claim("urn:altinn:authlevel", "5")
        }));

        // Act
        var result = claimsPrincipal.TryGetAuthenticationLevel(out var authenticationLevel);

        // Assert
        Assert.True(result);
        Assert.Equal(5, authenticationLevel);
    }
}
