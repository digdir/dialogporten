using Digdir.Domain.Dialogporten.GraphQL.EndUser.DialogById;
using HotChocolate.Authorization;
using HotChocolate.Language;
using Microsoft.AspNetCore.Authorization;

namespace Digdir.Domain.Dialogporten.GraphQL.Common.Extensions.HotChocolate;

public static class AuthorizationHandlerContextExtensions
{
    private const string DialogIdArgumentName = "dialogId";
    private const string DialogEventsFieldName = nameof(Subscriptions.DialogEvents);

    /// <summary>
    /// Attempts to extract the dialog ID from a DialogEvents subscription operation.
    /// </summary>
    /// <param name="context">The authorization handler context</param>
    /// <param name="dialogId">When this method returns, contains the extracted dialog ID if found; otherwise, Guid.Empty.</param>
    /// <returns>True if the dialog ID was successfully extracted; otherwise, false.</returns>
    public static bool TryGetDialogEventsSubscriptionDialogId(this AuthorizationHandlerContext context, out Guid dialogId)
    {
        dialogId = Guid.Empty;

        if (context.Resource is not AuthorizationContext authContext) return false;

        if (authContext.Document.Definitions.Count == 0) return false;

        var definition = authContext.Document.Definitions[0];

        if (definition is not OperationDefinitionNode operationDefinition) return false;

        if (operationDefinition.Operation != OperationType.Subscription) return false;

        var dialogEventsSelection = operationDefinition.SelectionSet.Selections.FirstOrDefault(x =>
            x is FieldNode fieldNode && fieldNode.Name.Value
                .Equals(DialogEventsFieldName, StringComparison.OrdinalIgnoreCase));

        if (dialogEventsSelection is not FieldNode fieldNode) return false;

        var dialogIdArgument = fieldNode.Arguments.FirstOrDefault(x => x.Name.Value.Equals(DialogIdArgumentName, StringComparison.OrdinalIgnoreCase));

        if (dialogIdArgument?.Value.Value is null) return false;

        return Guid.TryParse(dialogIdArgument.Value.Value.ToString(), out dialogId);
    }
}
