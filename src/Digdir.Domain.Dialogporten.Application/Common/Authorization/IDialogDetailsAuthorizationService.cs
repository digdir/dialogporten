using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

namespace Digdir.Domain.Dialogporten.Application.Common.Authorization;

internal interface IDialogDetailsAuthorizationService
{
    public Task<bool> PopulateAuthorizationFlags(DialogEntity dialogEntity, IUser user, CancellationToken cancellationToken = default);
}

internal class MockDialogDetailsAuthorizationService : IDialogDetailsAuthorizationService
{
    public async Task<bool> PopulateAuthorizationFlags(DialogEntity dialogEntity, IUser user, CancellationToken cancellationToken = default)
    {
        // TODO!
        // - Get all actions and elements for the dialog, with authorization resources (if supplied)
        // - Build a multi XACML request as per https://github.com/digdir/dialogporten/issues/43
        // - Send the request to Altinn
        // - Parse the response
        // - Populate the dialogEntity with the authorization flags based on the response
        return true;
    }
}
