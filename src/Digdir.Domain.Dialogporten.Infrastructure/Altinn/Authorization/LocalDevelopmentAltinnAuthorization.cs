using System.Diagnostics.CodeAnalysis;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Parties.Abstractions;
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
        Task.FromResult(new DialogDetailsAuthorizationResult { AuthorizedAltinnActions = dialogEntity.GetAltinnActions() });

    private static List<string> _allRolesCache = new();
    private static List<string> _allPartiesCache = new();
    private static List<string> _allResourcesCache = new();
    private static readonly Random Rnd = new();

    public async Task<DialogSearchAuthorizationResult> GetAuthorizedResourcesForSearch(List<string> constraintParties, List<string> serviceResources, string? endUserId,
        CancellationToken cancellationToken = default)
    {
        /*
        // constraintParties and serviceResources are passed from the client as query parameters
        // If one and/or the other is supplied, this will limit the resources and parties to the ones supplied
        var dialogData = await _db.Dialogs
            .Select(dialog => new { dialog.Party, dialog.ServiceResource })
            .WhereIf(constraintParties.Count != 0, dialog => constraintParties.Contains(dialog.Party))
            .WhereIf(serviceResources.Count != 0, dialog => serviceResources.Contains(dialog.ServiceResource))
            .Distinct()
            .ToListAsync(cancellationToken);

        // Keep the number of parties and resources reasonable
        var allParties = dialogData.Select(x => x.Party).Distinct().Take(1000).ToList();
        var allResources = dialogData.Select(x => x.ServiceResource).Distinct().Take(1000).ToList();
        var allRoles = await _db.SubjectResources.Select(x => x.Role).Distinct().Take(30).ToListAsync(cancellationToken);

        var authorizedResources = new DialogSearchAuthorizationResult
        {
            ResourcesByParties = allParties.ToDictionary(party => party, _ => allResources),
            RolesByParties = allParties.ToDictionary(party => party, _ => allRoles)
        };
        */

        if (_allRolesCache.Count == 0)
        {
            _allRolesCache = await _db.SubjectResources.Select(x => x.Role).Distinct().ToListAsync(cancellationToken);
            _allPartiesCache = await _db.Dialogs.Select(x => x.Party).Distinct().ToListAsync(cancellationToken);
            _allResourcesCache = await _db.Dialogs.Select(x => x.ServiceResource).Distinct().ToListAsync(cancellationToken);
        }

        var authorizedResources = new DialogSearchAuthorizationResult
        {
            ResourcesByParties = new(),
            RolesByParties = new()
        };

        // Get 10-30 random parties
        var numParties = Rnd.Next(10, 30);
        for (var i = 0; i < numParties; i++)
        {
            string party;
            do
            {
                party = _allPartiesCache[Rnd.Next(0, _allPartiesCache.Count - 1)];
            } while (authorizedResources.ResourcesByParties.ContainsKey(party));
            // Get 5-20 random resources
            var resources = new List<string>();
            var numResources = Rnd.Next(5, 20);
            for (var j = 0; j < numResources; j++)
            {
                var resource = _allResourcesCache[Rnd.Next(0, _allResourcesCache.Count - 1)];
                resources.Add(resource);
            }

            authorizedResources.ResourcesByParties.Add(party, resources);
        }

        // Get 10-30 random parties
        numParties = Rnd.Next(10, 30);
        for (var i = 0; i < numParties; i++)
        {
            string party;
            do
            {
                party = _allPartiesCache[Rnd.Next(0, _allPartiesCache.Count - 1)];
            } while (authorizedResources.RolesByParties.ContainsKey(party));

            // Get 10-30 random roles
            var roles = new List<string>();
            var numRoles = Rnd.Next(10, 30);
            for (var j = 0; j < numRoles; j++)
            {
                var role = _allRolesCache[Rnd.Next(0, _allRolesCache.Count - 1)];
                roles.Add(role);
            }

            authorizedResources.RolesByParties.Add(party, roles);
        }

        return authorizedResources;
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    [SuppressMessage("Style", "IDE0060:Remove unused parameter")]
    public async Task<AuthorizedPartiesResult> GetAuthorizedParties(IPartyIdentifier authenticatedParty, bool flatten = false, CancellationToken cancellationToken = default)
        => await Task.FromResult(new AuthorizedPartiesResult { AuthorizedParties = [new() { Name = "Local Party", Party = authenticatedParty.FullId }] });
}
