using Digdir.Domain.Dialogporten.GraphQL.EndUser.DialogById;
using HotChocolate.Language;

namespace Digdir.Domain.Dialogporten.GraphQL.Common.Extensions.HotChocolate;

public static class OperationDefinitionNodeExtensions
{
    public static bool TryGetDialogEventsSubscriptionDialogId(this OperationDefinitionNode definition, out Guid dialogId)
    {
        dialogId = Guid.Empty;

        var dialogEventsSelection = definition.SelectionSet.Selections.FirstOrDefault(x =>
            x is FieldNode fieldNode && fieldNode.Name.Value
                .Equals(nameof(Subscriptions.DialogEvents), StringComparison.OrdinalIgnoreCase));

        if (dialogEventsSelection is not FieldNode fieldNode) return false;

        var dialogIdArgument = fieldNode.Arguments.FirstOrDefault(x => x.Name.Value.Equals("dialogId", StringComparison.OrdinalIgnoreCase));

        if (dialogIdArgument?.Value.Value is null) return false;

        return Guid.TryParse(dialogIdArgument.Value.Value.ToString(), out dialogId);
    }
}
