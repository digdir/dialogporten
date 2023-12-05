using Digdir.Domain.Dialogporten.Application.Common.Authorization;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;

namespace Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;

public sealed class DialogDetailsAuthorizationResult
{
    // Each action applies to a resource. This is the main resource, or another resource indicated by a authorization attribute
    // eg. "urn:altinn:subresource:some-sub-resource" or "urn:altinn:task:task_1"
    public Dictionary<string, List<string>> AuthorizationAttributesByAuthorizedActions { get; init; } = new();

    public bool HasReadAccessToMainResource() =>
        AuthorizationAttributesByAuthorizedActions.ContainsKey(Constants.ReadAction) &&
        AuthorizationAttributesByAuthorizedActions[Constants.ReadAction].Contains(Constants.MainResource);

    public bool HasReadAccessToDialogElement(DialogElement dialogElement)
    {
        return dialogElement.AuthorizationAttribute is not null
            ? AuthorizationAttributesByAuthorizedActions.TryGetValue(Constants.ElementReadAction, out var authorizationAttributes) &&
              authorizationAttributes.Contains(dialogElement.AuthorizationAttribute)
            : HasReadAccessToMainResource();
    }
}
