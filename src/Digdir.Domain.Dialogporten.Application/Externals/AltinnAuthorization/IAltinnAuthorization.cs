using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

namespace Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;

public interface IAltinnAuthorization
{
    public Task<DialogDetailsAuthorizationResult> GetDialogDetailsAuthorization(
        DialogEntity dialogEntity,
        CancellationToken cancellationToken = default);

    public Task<DialogSearchAuthorizationResult> GetAuthorizedResourcesForSearch(
        List<string> constraintParties,
        List<string> constraintServiceResources,
        string? endUserId = null,
        CancellationToken cancellationToken = default);
}
