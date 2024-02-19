using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Xunit;

namespace Digdir.Domain.Dialogporten.Infrastructure.Unit.Tests;

public class QueryableExtensionsTests
{
    [Fact]
    public void WhereUserIsAuthorizedForFiltersDialogEntitiesCorrectly()
    {
        const string party1 = "Party1";
        const string party2 = "Party2";
        const string party3 = "Party3";
        const string resource1 = "Resource1";
        const string resource2 = "Resource2";
        const string resource3 = "Resource3";

        // Arrange
        var dialogEntities = new List<DialogEntity>
        {
            new() { Id = Guid.NewGuid(), Party = party1, ServiceResource = resource1 },
            new() { Id = Guid.NewGuid(), Party = party1, ServiceResource = resource2 },
            new() { Id = Guid.NewGuid(), Party = party2, ServiceResource = resource2 },
            new() { Id = Guid.NewGuid(), Party = party2, ServiceResource = resource3 },
            new() { Id = Guid.NewGuid(), Party = party1, ServiceResource = resource3 },
            new() { Id = Guid.NewGuid(), Party = party3, ServiceResource = resource3 }
        };

        var authorizedResources = new DialogSearchAuthorizationResult
        {
            DialogIds = [dialogEntities.First().Id],
            ResourcesByParties = new Dictionary<string, List<string>>
            {
                { party2, [resource2, resource3]}
            },
            PartiesByResources = new Dictionary<string, List<string>>
            {
                { resource2, [party1]}
            }
        };

        // Act
        var result = dialogEntities.AsQueryable().WhereUserIsAuthorizedFor(authorizedResources).ToList();

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains(dialogEntities[0], result); // authorized by id
        Assert.Contains(dialogEntities[1], result); // authorized to resource2 for party1
        Assert.Contains(dialogEntities[2], result); // authorized to party2 for resource2
        Assert.Contains(dialogEntities[3], result); // authorized to party2 for resource2
        Assert.DoesNotContain(dialogEntities[4], result); // not authorized to party1 for resource3 (or the other way around)
        Assert.DoesNotContain(dialogEntities[4], result); // not authorized to party3
    }
}
