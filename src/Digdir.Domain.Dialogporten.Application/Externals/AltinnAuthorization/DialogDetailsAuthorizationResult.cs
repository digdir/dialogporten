using Digdir.Domain.Dialogporten.Application.Common.Authorization;

namespace Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;

public sealed class DialogDetailsAuthorizationResult
{
    // Each action applies to a resource. This is the main resource, another subresource indicated by a authorization attribute
    // eg. "urn:altinn:subresource:some-sub-resource" or "urn:altinn:task:task_1", or another resource (ie. policy)
    // eg. urn:altinn:resource:some-other-resource
    public List<AltinnAction> AuthorizedAltinnActions { get; init; } = [];

    public bool HasReadAccessToMainResource() =>
        AuthorizedAltinnActions.Contains(new(Constants.ReadAction, Constants.MainResource));

    // TODO: Rename in https://github.com/digdir/dialogporten/issues/860
    // public bool HasReadAccessToDialogTransmission(DialogTransmission dialogTransmission) =>
    //     HasReadAccessToDialogTransmission(dialogTransmission.AuthorizationAttribute);
    //
    // public bool HasReadAccessToDialogTransmission(GetDialogDialogTransmissionDto dialogTransmission) =>
    //     HasReadAccessToDialogTransmission(dialogTransmission.AuthorizationAttribute);

    private bool HasReadAccessToDialogTransmission(string? authorizationAttribute)
    {
        return authorizationAttribute is not null
        // TODO: Rename in https://github.com/digdir/dialogporten/issues/860
            ? ( // Dialog transmissions are authorized by either the read or read action, depending on the authorization attribute type
                // The infrastructure will ensure that the correct action is used, so here we just check for either
                AuthorizedAltinnActions.Contains(new(Constants.TransmissionReadAction, authorizationAttribute))
                || AuthorizedAltinnActions.Contains(new(Constants.ReadAction, authorizationAttribute))
            ) : HasReadAccessToMainResource();
    }
}
