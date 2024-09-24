using Digdir.Domain.Dialogporten.GraphQL.EndUser.DialogById;
using HotChocolate.Language;

namespace Digdir.Domain.Dialogporten.GraphQL.Common.Extensions.HotChocolate;

public static class OperationDefinitionNodeExtensions
{
    private const string DialogIdArgumentName = "dialogId";
    private const string DialogEventsFieldName = nameof(Subscriptions.DialogEvents);

    /// <summary>
    /// Attempts to extract the dialog ID from a DialogEvents subscription operation.
    /// </summary>
    /// <param name="definition">The GraphQL operation definition node.</param>
    /// <param name="dialogId">When this method returns, contains the extracted dialog ID if found; otherwise, Guid.Empty.</param>
    /// <returns>True if the dialog ID was successfully extracted; otherwise, false.</returns>
    public static bool TryGetDialogEventsSubscriptionDialogId(this OperationDefinitionNode definition, out Guid dialogId)
    {
        dialogId = Guid.Empty;

        var dialogEventsSelection = definition.SelectionSet.Selections.FirstOrDefault(x =>
            x is FieldNode fieldNode && fieldNode.Name.Value
                .Equals(DialogEventsFieldName, StringComparison.OrdinalIgnoreCase));

        if (dialogEventsSelection is not FieldNode fieldNode) return false;

        var dialogIdArgument = fieldNode.Arguments.FirstOrDefault(x => x.Name.Value.Equals(DialogIdArgumentName, StringComparison.OrdinalIgnoreCase));

        if (dialogIdArgument?.Value.Value is null) return false;

        return Guid.TryParse(dialogIdArgument.Value.Value.ToString(), out dialogId);
    }
}
