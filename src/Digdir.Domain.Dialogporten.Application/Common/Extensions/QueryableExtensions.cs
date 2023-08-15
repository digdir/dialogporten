using System.Linq.Expressions;

namespace Digdir.Domain.Dialogporten.Application.Common.Extensions;

internal static class QueryableExtensions
{
    public static IQueryable<TSource> WhereIf<TSource>(this IQueryable<TSource> source, bool predicate, Expression<Func<TSource, bool>> queryPredicate)
    {
        return predicate ? source.Where(queryPredicate) : source;
    }
}
