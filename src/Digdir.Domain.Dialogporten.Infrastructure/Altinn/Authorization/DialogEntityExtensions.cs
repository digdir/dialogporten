using Digdir.Domain.Dialogporten.Application.Common.Authorization;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.Authorization;

public static class DialogEntityExtensions
{
    public static HashSet<AltinnAction> GetAltinnActions(this DialogEntity dialogEntity)
    {
        // Get all authorization attributes grouped by action defined on the dialogEntity, including both
        // apiActions and guiActions, as well as dialog elements with authorization attributes (which
        // require authorization for the action "elementread").
        return dialogEntity.ApiActions
            .Select(x => new AltinnAction(x.Action, x.AuthorizationAttribute))
            .Concat(dialogEntity.GuiActions
                .Select(x => new AltinnAction(x.Action, x.AuthorizationAttribute)))
            .Concat(dialogEntity.Elements
                .Where(x => x.AuthorizationAttribute is not null)
                .Select(x => new AltinnAction(Constants.ElementReadAction, x.AuthorizationAttribute)))
            // We always need to check if the user can read the main resource
            .Append(new AltinnAction(Constants.ReadAction, Constants.MainResource))
            .ToHashSet();
    }
}
