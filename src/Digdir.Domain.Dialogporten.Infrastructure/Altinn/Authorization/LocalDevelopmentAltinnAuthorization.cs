using System.Diagnostics.CodeAnalysis;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Infrastructure.Altinn.Authorization;

internal sealed class LocalDevelopmentAltinnAuthorization : IAltinnAuthorization
{
    private readonly IDialogDbContext _db;

    public LocalDevelopmentAltinnAuthorization(IDialogDbContext db)
    {
        _db = db;
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public Task<DialogDetailsAuthorizationResult> GetDialogDetailsAuthorization(DialogEntity dialogEntity,
        CancellationToken cancellationToken = default) =>
        PerformDialogDetailsAuthorization(new DialogDetailsAuthorizationRequest(), cancellationToken);

    public async Task<DialogSearchAuthorizationResult> GetAuthorizedResourcesForSearch(List<string> constraintParties, List<string> serviceResources,
        CancellationToken cancellationToken = default) =>
        await PerformDialogSearchAuthorization(new DialogSearchAuthorizationRequest(), cancellationToken);

    private static Task<DialogDetailsAuthorizationResult> PerformDialogDetailsAuthorization(DialogDetailsAuthorizationRequest request,
        CancellationToken _ = default) =>
        // Just allow everything
        Task.FromResult(new DialogDetailsAuthorizationResult
        {
            AuthorizedActions = request.Actions
        });

    private async Task<DialogSearchAuthorizationResult> PerformDialogSearchAuthorization(DialogSearchAuthorizationRequest _,
        CancellationToken cancellationToken = default)
    {
        // Allow all resources for all parties
        var allParties = await _db.Dialogs
            .Select(dialog => dialog.Party)
            .Distinct()
            .ToListAsync(cancellationToken: cancellationToken);

        var allResources = await _db.Dialogs
            .Select(x => x.ServiceResource)
            .Distinct().ToListAsync(cancellationToken: cancellationToken);

        var authorizedResources = new DialogSearchAuthorizationResult();

        if (allParties.Count <= allResources.Count)
        {
            authorizedResources.PartiesForResources = allResources.ToDictionary(resource => resource, resource => allParties);
        }
        else
        {
            authorizedResources.ResourcesForParties = allParties.ToDictionary(party => party, party => allResources);
        }

        return authorizedResources;
    }
}

/*

 AND (
   (d."Party" = @__party_0 AND d."ServiceResource" = ANY (@__resources_1))
OR (d."Party" = @__party_2 AND d."ServiceResource" = ANY (@__resources_3))
OR (d."Party" = @__party_4 AND d."ServiceResource" = ANY (@__resources_5))
)
 *
 *
 */
