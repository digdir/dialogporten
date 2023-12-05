using Digdir.Domain.Dialogporten.Application.Common.Authorization;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.Authorization;

public static class DialogEntityExtensions
{
    public static Dictionary<string, List<string>> GetAuthorizationAttributesByAction(this DialogEntity dialogEntity)
    {
        // Get all authorization attributes grouped by action defined on the dialogEntity, including both
        // apiActions and guiActions, as well as dialog elements with authorization attributes (which
        // require authorization for the action "elementread").
        var actions = dialogEntity.ApiActions
            .Select(x => new { x.Action, x.AuthorizationAttribute })
            .Concat(dialogEntity.GuiActions
                .Select(x => new { x.Action, x.AuthorizationAttribute }))
            .Concat(dialogEntity.Elements
                .Where(x => x.AuthorizationAttribute is not null)
                .Select(x => new { Action = Constants.ElementReadAction, x.AuthorizationAttribute }))
            .GroupBy(x => x.Action)
            .ToDictionary(
                keySelector: x => x.Key,
                elementSelector: x => x
                    .Select(x => x.AuthorizationAttribute ?? Constants.MainResource)
                    .Distinct()
                    .ToList()
            );

        // We always need to check if the user can read the main resource
        if (!actions.TryGetValue(Constants.ReadAction, out var authorizationAttributes))
        {
            authorizationAttributes = new List<string>();
            actions.Add(Constants.ReadAction, authorizationAttributes);
        }
        authorizationAttributes.Add(Constants.MainResource);

        return actions;
    }
}
