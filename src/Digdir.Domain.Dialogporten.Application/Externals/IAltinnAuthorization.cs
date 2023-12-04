using System.Security.Claims;
using Digdir.Domain.Dialogporten.Application.Features.V1.Authorization;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface IAltinnAuthorization
{
    public Task<DialogDetailsAuthorizationResult> GetDialogDetailsAuthorization(
        DialogEntity dialogEntity,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken = default);

    public Task<DialogSearchAuthorizationResult> GetAuthorizedResourcesForSearch(
        List<string> constraintParties,
        List<string> serviceResources,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken = default);

    public Task<DialogSearchAuthorizationResult> PerformDialogSearchAuthorization(
        DialogSearchAuthorizationRequest request,
        CancellationToken cancellationToken = default);

    public Task<DialogDetailsAuthorizationResult> PerformDialogDetailsAuthorization(
        DialogDetailsAuthorizationRequest request,
        CancellationToken cancellationToken = default);
}
