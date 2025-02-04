using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.GraphQL.EndUser.Common;

namespace Digdir.Domain.Dialogporten.GraphQl.Unit.Tests.ObjectTypes;

public class ActivityTests
{
    [Fact]
    public void Activity_Types_In_GraphQl_Must_Match_Domain_Types()
    {
        // Arrange
        var domainTypes = Enum.GetValues(typeof(DialogActivityType.Values)).Cast<DialogActivityType.Values>().ToList();
        var graphQlTypes = Enum.GetValues(typeof(ActivityType)).Cast<ActivityType>().ToList();

        // Assert
        Assert.Equal(domainTypes.Count, graphQlTypes.Count);

        for (var i = 0; i < domainTypes.Count; i++)
        {
            Assert.Equal(domainTypes[i].ToString(), graphQlTypes[i].ToString());
        }
    }
}
