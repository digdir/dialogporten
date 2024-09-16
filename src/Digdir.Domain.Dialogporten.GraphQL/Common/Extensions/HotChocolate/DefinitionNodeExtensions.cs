using HotChocolate.Language;

namespace Digdir.Domain.Dialogporten.GraphQL.Common.Extensions.HotChocolate;

public static class DefinitionNodeExtensions
{
    public static bool TryGetSubscriptionDialogId(this IReadOnlyList<IDefinitionNode> definitions, out Guid dialogId)
    {
        dialogId = Guid.Empty;

        foreach (var definition in definitions)
        {
            if (definition is not OperationDefinitionNode operationDefinition)
            {
                continue;
            }

            if (operationDefinition.Operation != OperationType.Subscription)
            {
                continue;
            }

            if (operationDefinition.SelectionSet.Selections[0] is not FieldNode fieldNode)
            {
                continue;
            }

            var dialogIdArgument = fieldNode.Arguments.SingleOrDefault(x => x.Name.Value == "dialogId");

            if (dialogIdArgument is null)
            {
                continue;
            }

            if (dialogIdArgument.Value.Value is null)
            {
                continue;
            }

            if (Guid.TryParse(dialogIdArgument.Value.Value.ToString(), out dialogId))
            {
                return true;
            }
        }

        return false;
    }
}
