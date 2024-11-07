using Digdir.Domain.Dialogporten.Application.Common;
using FluentAssertions;

namespace Digdir.Domain.Dialogporten.Application.Unit.Tests;

public class ApplicationEventHandlerUtilsTests
{
    [Fact]
    public void Can_Not_Have_Duplicate_Endpoint_Names_On_Domain_Event_Handlers()
    {
        // Act
        var sameNameEndpoints = ApplicationEventHandlerUtils
            .GetHandlerEventMaps()
            .Select(x => EndpointNameAttribute.GetName(x.HandlerType, x.EventType))
            .GroupBy(x => x)
            .Where(x => x.Count() > 1)
            .ToArray();

        // Assert
        sameNameEndpoints.Should().BeEmpty();
    }

    /// <summary>
    /// Did you break this test, sir? If so, you should use caution when renaming endpoints.
    /// </summary>
    [Fact]
    public async Task Developer_Should_Use_Caution_When_Renaming_Endpoints()
    {
        // Act
        var map = ApplicationEventHandlerUtils
            .GetHandlerEventMaps()
            .Select(x => EndpointNameAttribute.GetName(x.HandlerType, x.EventType))
            .ToArray();

        // Assert
        await Verify(map).UseDirectory("Snapshots");
    }
}
