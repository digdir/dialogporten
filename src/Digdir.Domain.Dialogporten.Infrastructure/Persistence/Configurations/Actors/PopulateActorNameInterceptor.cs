using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Actors;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Digdir.Domain.Dialogporten.Infrastructure.DomainEvents.Outbox;

internal sealed class PopulateActorNameInterceptor : SaveChangesInterceptor
{
    private static readonly Type ActorType = typeof(Actor);

    private readonly IDomainContext _domainContext;
    private readonly IPartyNameRegistry _partyNameRegistry;

    public PopulateActorNameInterceptor(
        IDomainContext domainContext,
        IPartyNameRegistry partyNameRegistry)
    {
        _domainContext = domainContext ?? throw new ArgumentNullException(nameof(domainContext));
        _partyNameRegistry = partyNameRegistry ?? throw new ArgumentNullException(nameof(partyNameRegistry));
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;

        if (dbContext is null)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var actors = dbContext.ChangeTracker.Entries()
            .Where(x => x.Metadata.ClrType.IsAssignableTo(ActorType))
            .Where(x => x.State is EntityState.Added or EntityState.Modified)
            .Select(x =>
            {
                var actor = (Actor)x.Entity;
                actor.ActorId = actor.ActorId?.ToLowerInvariant();
                return actor;
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.ActorId))
            .ToList();

        var actorNameById = new Dictionary<string, string?>();
        foreach (var actorId in actors
                     .Select(x => x.ActorId!)
                     .Distinct())
        {
            actorNameById[actorId] = await _partyNameRegistry.GetName(actorId, cancellationToken);
        }

        foreach (var actor in actors)
        {
            if (!actorNameById.TryGetValue(actor.ActorId!, out var actorName) || string.IsNullOrWhiteSpace(actorName))
            {
                // We don't want to fail the save operation if we are unable to look up the
                // name for this particular actor, as it is used on enduser get operations.
                if (actor is DialogSeenLogSeenByActor)
                {
                    continue;
                }

                _domainContext.AddError(nameof(Actor.ActorId), $"Unable to look up name for actor id: {actor.ActorId}");
                continue;
            }

            actor.ActorName = actorName;
        }

        return !_domainContext.IsValid
            ? InterceptionResult<int>.SuppressWithResult(0)
            : await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
