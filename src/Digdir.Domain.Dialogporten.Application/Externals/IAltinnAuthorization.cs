using Digdir.Domain.Dialogporten.Domain.Authorization;

namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface IAltinnAuthorization
{
    public Task<DialogSearchAuthorizationResponse> PerformDialogSearchAuthorization(DialogSearchAuthorizationRequest request, CancellationToken cancellationToken);
    public Task<DialogDetailsAuthorizationResponse> PerformDialogDetailsAuthorization(DialogDetailsAuthorizationRequest request, CancellationToken cancellationToken);
}
