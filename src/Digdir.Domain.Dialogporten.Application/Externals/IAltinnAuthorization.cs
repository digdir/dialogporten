using Digdir.Domain.Dialogporten.Domain.Authorization;

namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface IAltinnAuthorization
{
    public Task<DialogSearchAuthorizationResult> PerformDialogSearchAuthorization(DialogSearchAuthorizationRequest request, CancellationToken cancellationToken);
    public Task<DialogDetailsAuthorizationResult> PerformDialogDetailsAuthorization(DialogDetailsAuthorizationRequest request, CancellationToken cancellationToken);
}
