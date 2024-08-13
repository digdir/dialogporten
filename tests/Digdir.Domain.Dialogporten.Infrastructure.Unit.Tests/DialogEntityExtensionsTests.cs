using Digdir.Domain.Dialogporten.Application.Common.Authorization;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Infrastructure.Altinn.Authorization;
using Xunit;

namespace Digdir.Domain.Dialogporten.Infrastructure.Unit.Tests;

public class DialogEntityExtensionsTests
{
    [Fact]
    public void GetAltinnActionsShouldReturnCorrectActionsForTransmissionAuthorizationAttributes()
    {
        // Arrange
        var dialogEntity = new DialogEntity
        {
            ApiActions = [],
            GuiActions = [],
            Transmissions =
            [
                new() { AuthorizationAttribute = "foo" },
                new() { AuthorizationAttribute = "urn:altinn:subresource:bar" },
                new() { AuthorizationAttribute = "urn:altinn:task:Task_1" },
                new() { AuthorizationAttribute = "urn:altinn:resource:some-service:element1" },
                new() { AuthorizationAttribute = "urn:altinn:resource:app_ttd_some-app" }
            ]
        };

        // Act
        var actions = dialogEntity.GetAltinnActions();

        // Assert
        Assert.NotNull(actions);
        Assert.NotEmpty(actions);
        Assert.Contains(actions, a => a is { Name: Constants.TransmissionReadAction, AuthorizationAttribute: "foo" });
        Assert.Contains(actions, a => a is { Name: Constants.TransmissionReadAction, AuthorizationAttribute: "urn:altinn:subresource:bar" });
        Assert.Contains(actions, a => a is { Name: Constants.TransmissionReadAction, AuthorizationAttribute: "urn:altinn:task:Task_1" });
        Assert.Contains(actions, a => a is { Name: Constants.ReadAction, AuthorizationAttribute: "urn:altinn:resource:some-service:element1" });
        Assert.Contains(actions, a => a is { Name: Constants.ReadAction, AuthorizationAttribute: "urn:altinn:resource:app_ttd_some-app" });
    }
}
