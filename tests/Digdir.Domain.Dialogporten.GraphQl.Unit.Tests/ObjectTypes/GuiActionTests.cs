using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.GraphQL.EndUser.DialogById;

namespace Digdir.Domain.Dialogporten.GraphQl.Unit.Tests.ObjectTypes;

public class GuiActionTests
{
    [Fact]
    public void DialogGuiAction_Object_Type_Should_Match_Property_Names_On_GuiAction()
    {
        // Arrange
        var ignoreList = new List<string>
        {
            nameof(DialogGuiAction.CreatedAt),
            nameof(DialogGuiAction.UpdatedAt),
            nameof(DialogGuiAction.PriorityId),
            nameof(DialogGuiAction.HttpMethodId),
            nameof(DialogGuiAction.DialogId),
            nameof(DialogGuiAction.Dialog)
        };

        var domainGuiActionProperties = typeof(DialogGuiAction)
            .GetProperties()
            .Select(p => p.Name)
            .Where(name => !ignoreList.Contains(name))
            .ToList();

        var guiActionProperties = typeof(GuiAction)
            .GetProperties()
            .Select(p => p.Name)
            .ToList();

        var missingProperties = domainGuiActionProperties
            .Except(guiActionProperties, StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Assert
        Assert.True(missingProperties.Count == 0,
            $"Properties missing in graphql GuiAction: {string.Join(", ", missingProperties)}");
    }

    [Fact]
    public void GuiActionPriority_Object_Type_Should_Match_Property_Names_On_DialogGuiActionPriorityValues()
    {
        // Arrange
        var guiActionPriorities = typeof(GuiActionPriority)
            .GetProperties()
            .Select(p => p.Name)
            .ToList();

        var domainGuiActionPriorities = typeof(DialogGuiActionPriority.Values)
            .GetProperties()
            .Select(p => p.Name)
            .ToList();

        var missingProperties = domainGuiActionPriorities
            .Except(guiActionPriorities, StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Assert
        Assert.True(missingProperties.Count == 0,
            $"Properties missing in graphql GuiActionPriority: {string.Join(", ", missingProperties)}");
    }
}
