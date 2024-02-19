using Digdir.Domain.Dialogporten.Application.Common.Authorization;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Elements;

namespace Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;

public sealed class DialogDetailsAuthorizationResult
{
    // Each action applies to a resource. This is the main resource, or another resource indicated by a authorization attribute
    // eg. "urn:altinn:subresource:some-sub-resource" or "urn:altinn:task:task_1"
    public HashSet<AltinnAction> AuthorizedAltinnActions { get; init; } = [];

    public bool HasReadAccessToMainResource() =>
        AuthorizedAltinnActions.Contains(new(Constants.ReadAction, Constants.MainResource));

    public bool HasReadAccessToDialogElement(DialogElement dialogElement)
    {
        return dialogElement.AuthorizationAttribute is not null
            ? AuthorizedAltinnActions.Contains(new(Constants.ElementReadAction, dialogElement.AuthorizationAttribute))
            : HasReadAccessToMainResource();
    }
}
