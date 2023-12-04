using System.Security.Claims;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Features.V1.Authorization;
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

    public Task<DialogDetailsAuthorizationResult> GetDialogDetailsAuthorization(DialogEntity dialogEntity, ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken = default) =>
        PerformDialogDetailsAuthorization(new DialogDetailsAuthorizationRequest(), cancellationToken);

    public async Task<DialogSearchAuthorizationResult> GetAuthorizedResourcesForSearch(List<string> constraintParties, List<string> serviceResources, ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken = default) =>
        await PerformDialogSearchAuthorization(new DialogSearchAuthorizationRequest(), cancellationToken);

    public Task<DialogDetailsAuthorizationResult> PerformDialogDetailsAuthorization(DialogDetailsAuthorizationRequest request,
        CancellationToken cancellationToken = default) =>
        // Just allow everything
        Task.FromResult(new DialogDetailsAuthorizationResult
        {
            AuthorizedActions = request.Actions
        });

    public async Task<DialogSearchAuthorizationResult> PerformDialogSearchAuthorization(DialogSearchAuthorizationRequest request,
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
