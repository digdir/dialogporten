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
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public Task<DialogDetailsAuthorizationResult> GetDialogDetailsAuthorization(DialogEntity dialogEntity,
        CancellationToken cancellationToken = default) =>
        // Just allow everything
        Task.FromResult(new DialogDetailsAuthorizationResult { AuthorizationAttributesByAuthorizedActions = new() });

    public async Task<DialogSearchAuthorizationResult> GetAuthorizedResourcesForSearch(List<string> constraintParties, List<string> serviceResources,
        CancellationToken cancellationToken = default)
    {
        // Allow all resources for all parties
        var dialogData = await _db.Dialogs
            .Select(dialog => new { dialog.Party, dialog.ServiceResource })
            .Distinct()
            .ToListAsync(cancellationToken);

        var allParties = dialogData.Select(x => x.Party).Distinct().ToList();
        var allResources = dialogData.Select(x => x.ServiceResource).Distinct().ToList();

        var authorizedResources = new DialogSearchAuthorizationResult
        {
            PartiesByResources = allResources.ToDictionary(resource => resource, resource => allParties),
            ResourcesByParties = allParties.ToDictionary(party => party, party => allResources)
        };

        return authorizedResources;
    }
}
