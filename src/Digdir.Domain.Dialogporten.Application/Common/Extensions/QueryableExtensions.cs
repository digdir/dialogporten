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

    // TODO! This is mostly courtesy of ChatGPT as of now. We might want to look into Linqkit or something similar to make this more readable.
    // Performance might also not be stellar.
    public static IQueryable<TSource> WhereUserIsAuthorizedFor<TSource>(this IQueryable<TSource> source, AuthorizedResources authorizedResources)
        where TSource : DialogEntity
    {
        // Start with an initial predicate that includes the dialog IDs.
        Expression<Func<TSource, bool>> predicate = x => authorizedResources.DialogIds.Contains(x.Id);

        // Adding conditions for ResourcesForParties
        if (authorizedResources.ResourcesForParties.Any())
        {
            var partyPredicate = PredicateFalse<TSource>();
            foreach (var kvp in authorizedResources.ResourcesForParties)
            {
                var party = kvp.Key;
                var resources = kvp.Value;
                partyPredicate = Or(partyPredicate, x => x.Party == party && resources.Contains(x.ServiceResource));
            }
            predicate = Or(predicate, partyPredicate);
        }

        // Adding conditions for PartiesForResources
        if (authorizedResources.PartiesForResources.Any())
        {
            var resourcePredicate = PredicateFalse<TSource>();
            foreach (var kvp in authorizedResources.PartiesForResources)
            {
                var resource = kvp.Key;
                var parties = kvp.Value;
                resourcePredicate = Or(resourcePredicate, x => x.ServiceResource == resource && parties.Contains(x.Party));
            }
            predicate = Or(predicate, resourcePredicate);
        }

        return source.Where(predicate);
    }

    private static Expression<Func<T, bool>> PredicateFalse<T>()
    {
        return x => false;
    }

    private static Expression<Func<T, bool>> Or<T>(Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
    {
        var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
        return Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
    }
}
