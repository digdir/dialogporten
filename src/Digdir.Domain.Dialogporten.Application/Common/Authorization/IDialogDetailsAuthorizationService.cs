using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Authorization;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

namespace Digdir.Domain.Dialogporten.Application.Common.Authorization;

internal interface IDialogDetailsAuthorizationService
{
    public Task<DialogDetailsAuthorizationResponse> GetDialogDetailsAuthorization(DialogEntity dialogEntity, CancellationToken cancellationToken = default);
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

    public async Task<DialogDetailsAuthorizationResponse> GetDialogDetailsAuthorization(DialogEntity dialogEntity, CancellationToken cancellationToken = default)
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
