using System.Linq.Expressions;
using Digdir.Domain.Dialogporten.Application.Common.Authorization;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

namespace Digdir.Domain.Dialogporten.Application.Common.Extensions;

internal static class QueryableExtensions
{
    public static IQueryable<TSource> WhereIf<TSource>(this IQueryable<TSource> source, bool predicate, Expression<Func<TSource, bool>> queryPredicate)
    {
        return predicate ? source.Where(queryPredicate) : source;
    }

    public static IQueryable<DialogEntity> WhereUserIsAuthorizedFor(this IQueryable<DialogEntity> source, AuthorizedResources authorizedResources)
    {
        var predicate = Expressions.Boolean<DialogEntity>.True;

        if (authorizedResources.DialogIds.Any())
        {
            predicate = x => authorizedResources.DialogIds.Contains(x.Id);
        }

        if (authorizedResources.ResourcesForParties.Any())
        {
            var partyPredicate = Expressions.Boolean<DialogEntity>.False;
            foreach (var (party, resources) in authorizedResources.ResourcesForParties)
            {
                partyPredicate = Expressions.Or(partyPredicate, x => x.Party == party && resources.Contains(x.ServiceResource));
            }
            predicate = Expressions.Or(predicate, partyPredicate);
        }

        if (authorizedResources.PartiesForResources.Any())
        {
            var resourcePredicate = Expressions.Boolean<DialogEntity>.False;
            foreach (var (resource, parties) in authorizedResources.PartiesForResources)
            {
                resourcePredicate = Expressions.Or(resourcePredicate, x => x.ServiceResource == resource && parties.Contains(x.Party));
            }
            predicate = Expressions.Or(predicate, resourcePredicate);
        }

        return source.Where(predicate);
    }
}
