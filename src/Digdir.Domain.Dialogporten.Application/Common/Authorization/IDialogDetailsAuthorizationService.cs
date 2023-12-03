using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogElements.Queries.Get;
using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.Domain.Authorization;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

namespace Digdir.Domain.Dialogporten.Application.Common.Authorization;

internal interface IDialogDetailsAuthorizationService
{
    public Task<DialogDetailsAuthorizationResult> GetDialogDetailsAuthorization(DialogEntity dialogEntity, CancellationToken cancellationToken = default);

    public void DecorateWithAuthorization(GetDialogDto dto, DialogDetailsAuthorizationResult authorizationResult);
    public void DecorateWithAuthorization(GetDialogElementDto dto, DialogDetailsAuthorizationResult authorizationResult);
}

internal sealed class DialogDetailsAuthorizationService : IDialogDetailsAuthorizationService
{
    private readonly IAltinnAuthorization _altinnAuthorization;
    private readonly IUserService _userService;

    private const string ElementReadAction = "elementread";


    public DialogDetailsAuthorizationService(
        IAltinnAuthorization altinnAuthorization,
        IUserService userService)
    {
        _altinnAuthorization = altinnAuthorization ?? throw new ArgumentNullException(nameof(altinnAuthorization));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task<DialogDetailsAuthorizationResult> GetDialogDetailsAuthorization(DialogEntity dialogEntity, CancellationToken cancellationToken = default)
    {

        var dialogDetailsAuthorizationRequest = new DialogDetailsAuthorizationRequest
        {
            ClaimsPrincipal = _userService.GetCurrentUser().GetPrincipal(),
            ServiceResource = dialogEntity.ServiceResource,
            DialogId = dialogEntity.Id,
            Party = dialogEntity.Party
        };

        PopulateRequestedActions(dialogEntity, dialogDetailsAuthorizationRequest);
        PopulateRequestsDialogElements(dialogEntity, dialogDetailsAuthorizationRequest);

        return await _altinnAuthorization.PerformDialogDetailsAuthorization(dialogDetailsAuthorizationRequest, cancellationToken);
    }

    public void DecorateWithAuthorization(GetDialogDto dto, DialogDetailsAuthorizationResult authorizationResult)
    {
        foreach (var (action, resources) in authorizationResult.AuthorizedActions)
        {
            foreach (var apiAction in dto.ApiActions.Where(a => a.Action == action))
            {
                if ((apiAction.AuthorizationAttribute is null && resources.Contains(DialogDetailsAuthorizationResult.MainResource))
                    || (apiAction.AuthorizationAttribute is not null && resources.Contains(apiAction.AuthorizationAttribute)))
                {
                    apiAction.IsAuthorized = true;
                }
            }

            foreach (var guiAction in dto.GuiActions.Where(a => a.Action == action))
            {
                if ((guiAction.AuthorizationAttribute is null && resources.Contains(DialogDetailsAuthorizationResult.MainResource))
                    || (guiAction.AuthorizationAttribute is not null && resources.Contains(guiAction.AuthorizationAttribute)))
                {
                    guiAction.IsAuthorized = true;
                }
            }

            // Any action will give access to a diaog element, unless a authorization attribute is set, in which case
            // an "elementread" action is required
            foreach (var dialogElement in dto.Elements.Where(dialogElement => (dialogElement.AuthorizationAttribute is null)
                         || (dialogElement.AuthorizationAttribute is not null && action == ElementReadAction)))
            {
                dialogElement.IsAuthorized = true;
            }
        }
    }

    public void DecorateWithAuthorization(GetDialogElementDto dto, DialogDetailsAuthorizationResult authorizationResult)
    {
        if (authorizationResult.AuthorizedActions.Count == 0)
        {
            return;
        }

        // Any action will give access to a diaog element, unless a authorization attribute is set, in which case
        // an "elementread" action is required
        if (dto.AuthorizationAttribute == null ||
            (authorizationResult.AuthorizedActions.TryGetValue(ElementReadAction, out var authorizedAttributesForElementRead) && authorizedAttributesForElementRead.Contains(dto.AuthorizationAttribute)))
        {
            dto.IsAuthorized = true;
        }
    }

    private static void PopulateRequestsDialogElements(DialogEntity dialogEntity,
        DialogDetailsAuthorizationRequest dialogDetailsAuthorizationRequest)
    {
        foreach (var dialogElement in dialogEntity.Elements.Where(dialogElement =>
                     dialogElement.AuthorizationAttribute != null))
        {
            if (!dialogDetailsAuthorizationRequest.Actions.ContainsKey(ElementReadAction))
            {
                dialogDetailsAuthorizationRequest.Actions.Add(ElementReadAction, new List<string>());
            }

            if (!dialogDetailsAuthorizationRequest.Actions[ElementReadAction].Contains(dialogElement.AuthorizationAttribute!))
            {
                dialogDetailsAuthorizationRequest.Actions[ElementReadAction].Add(dialogElement.AuthorizationAttribute!);
            }

        }
    }

    private static void PopulateRequestedActions(DialogEntity dialogEntity,
        DialogDetailsAuthorizationRequest dialogDetailsAuthorizationRequest)
    {
        foreach (var action in dialogEntity.ApiActions)
        {
            if (!dialogDetailsAuthorizationRequest.Actions.ContainsKey(action.Action))
            {
                dialogDetailsAuthorizationRequest.Actions.Add(action.Action, new List<string>());
            }

            var resource = action.AuthorizationAttribute ?? DialogDetailsAuthorizationRequest.MainResource;
            if (!dialogDetailsAuthorizationRequest.Actions[action.Action].Contains(resource))
            {
                dialogDetailsAuthorizationRequest.Actions[action.Action].Add(resource);
            }
        }

        foreach (var action in dialogEntity.GuiActions)
        {
            if (!dialogDetailsAuthorizationRequest.Actions.ContainsKey(action.Action))
            {
                dialogDetailsAuthorizationRequest.Actions.Add(action.Action, new List<string>());
            }

            var resource = action.AuthorizationAttribute ?? DialogDetailsAuthorizationRequest.MainResource;
            if (!dialogDetailsAuthorizationRequest.Actions[action.Action].Contains(resource))
            {
                dialogDetailsAuthorizationRequest.Actions[action.Action].Add(resource);
            }
        }
    }
}
