using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Dialogs.Queries.Search;
using Digdir.Domain.Dialogporten.Domain.Authorization;

namespace Digdir.Domain.Dialogporten.Application.Common.Authorization;

internal interface IDialogSearchAuthorizationService
{
    public Task<DialogSearchAuthorizationResult> GetAuthorizedResourcesForSearch(SearchDialogQuery request, CancellationToken cancellationToken = default);
}

internal sealed class DialogSearchAuthorizationService : IDialogSearchAuthorizationService
{
    private readonly IUserService _userService;
    private readonly IAltinnAuthorization _altinnAuthorization;

    public DialogSearchAuthorizationService(IUserService userService, IAltinnAuthorization altinnAuthorization)
    {
        _userService = userService;
        _altinnAuthorization = altinnAuthorization;
    }

    public async Task<DialogSearchAuthorizationResult> GetAuthorizedResourcesForSearch(SearchDialogQuery request, CancellationToken cancellationToken = default)
    {
        var authRequest = new DialogSearchAuthorizationRequest
        {
            ClaimsPrincipal = _userService.GetCurrentUser().GetPrincipal(),
            ConstraintParties = request.Party,
            ConstraintServiceResources = request.ServiceResource
        };

        return await _altinnAuthorization.PerformDialogSearchAuthorization(authRequest, cancellationToken);
    }
}

