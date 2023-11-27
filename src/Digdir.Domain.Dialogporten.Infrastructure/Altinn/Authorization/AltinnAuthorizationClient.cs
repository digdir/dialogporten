using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Authorization;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.Authorization;

internal sealed class AltinnAuthorizationClient : IAltinnAuthorization
{
    public Task<DialogSearchAuthorizationResponse> PerformDialogSearchAuthorization(DialogSearchAuthorizationRequest request, CancellationToken cancellationToken)
    {
        // TODO
        // - Implement as per https://github.com/digdir/dialogporten/issues/249
        // - Note that either ServiceResource or Party is always supplied in the request.
        // - Whether or not to populate ResourcesForParties or PartiesForResources depends on which one is supplied in the request.
        // - The user is also always authorized for its own dialogs, which might be an optimization

        throw new NotImplementedException();
    }

    public Task<DialogDetailsAuthorizationResponse> PerformDialogDetailsAuthorization(DialogDetailsAuthorizationRequest request, CancellationToken cancellationToken)
    {
        // TODO!
        // - Get all actions and elements for the dialog, with authorization resources (if supplied)
        // - Build a multi XACML request as per https://github.com/digdir/dialogporten/issues/43
        // - Send the request to Altinn
        // - Parse the response
        // - Build and return a DialogDetailsAuthorizationResponse

        throw new NotImplementedException();
    }
}
