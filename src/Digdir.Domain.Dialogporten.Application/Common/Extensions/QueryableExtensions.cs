using System.Linq.Expressions;
using System.Reflection;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

namespace Digdir.Domain.Dialogporten.Application.Common.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<TSource> WhereIf<TSource>(this IQueryable<TSource> source, bool predicate, Expression<Func<TSource, bool>> queryPredicate)
        => predicate ? source.Where(queryPredicate) : source;

    private static readonly Type DialogType = typeof(DialogEntity);
    private static readonly PropertyInfo DialogIdPropertyInfo = DialogType.GetProperty(nameof(DialogEntity.Id))!;
    private static readonly PropertyInfo DialogPartyPropertyInfo = DialogType.GetProperty(nameof(DialogEntity.Party))!;
    private static readonly PropertyInfo DialogServiceResourcePropertyInfo = DialogType.GetProperty(nameof(DialogEntity.ServiceResource))!;

    private static readonly MethodInfo StringListContainsMethodInfo = typeof(List<string>).GetMethod(nameof(List<object>.Contains))!;
    private static readonly MethodInfo GuidListContainsMethodInfo = typeof(List<Guid>).GetMethod(nameof(List<object>.Contains))!;

    private static readonly Type KeyValueType = typeof(KeyValuePair<string, List<string>>);
    private static readonly PropertyInfo KeyPropertyInfo = KeyValueType.GetProperty(nameof(KeyValuePair<object, object>.Key))!;
    private static readonly PropertyInfo ValuePropertyInfo = KeyValueType.GetProperty(nameof(KeyValuePair<object, object>.Value))!;

    private static readonly Type DialogSearchAuthorizationResultType = typeof(DialogSearchAuthorizationResult);
    private static readonly PropertyInfo DialogIdsPropertyInfo = DialogSearchAuthorizationResultType.GetProperty(nameof(DialogSearchAuthorizationResult.DialogIds))!;

    public static IQueryable<DialogEntity> WhereUserIsAuthorizedFor(
        this IQueryable<DialogEntity> source,
        DialogSearchAuthorizationResult authorizedResources)
    {
        if (authorizedResources.HasNoAuthorizations)
        {
            return source.Where(x => false);
        }

        var dialogParameter = Expression.Parameter(DialogType);
        var id = Expression.MakeMemberAccess(dialogParameter, DialogIdPropertyInfo);
        var party = Expression.MakeMemberAccess(dialogParameter, DialogPartyPropertyInfo);
        var serviceResource = Expression.MakeMemberAccess(dialogParameter, DialogServiceResourcePropertyInfo);

        var partyResourceExpressions = new List<Expression>();

        foreach (var item in authorizedResources.ResourcesByParties)
        {
            var itemArg = Expression.Constant(item, KeyValueType);
            var partyAccess = Expression.MakeMemberAccess(itemArg, KeyPropertyInfo);
            var resourcesAccess = Expression.MakeMemberAccess(itemArg, ValuePropertyInfo);
            var partyEquals = Expression.Equal(partyAccess, party);
            var resourceContains = Expression.Call(resourcesAccess, StringListContainsMethodInfo, serviceResource);
            partyResourceExpressions.Add(Expression.AndAlso(partyEquals, resourceContains));
        }

        foreach (var item in authorizedResources.PartiesByResources)
        {
            var itemArg = Expression.Constant(item, KeyValueType);
            var resourceAccess = Expression.MakeMemberAccess(itemArg, KeyPropertyInfo);
            var partiesAccess = Expression.MakeMemberAccess(itemArg, ValuePropertyInfo);
            var resourceEquals = Expression.Equal(resourceAccess, serviceResource);
            var partiesContains = Expression.Call(partiesAccess, StringListContainsMethodInfo, party);
            partyResourceExpressions.Add(Expression.AndAlso(resourceEquals, partiesContains));
        }

        if (authorizedResources.DialogIds.Count > 0)
        {
            var itemArg = Expression.Constant(authorizedResources, DialogSearchAuthorizationResultType);
            var dialogIdsAccess = Expression.MakeMemberAccess(itemArg, DialogIdsPropertyInfo);
            var dialogIdsContains = Expression.Call(dialogIdsAccess, GuidListContainsMethodInfo, id);
            partyResourceExpressions.Add(dialogIdsContains);
        }

        var predicate = partyResourceExpressions
            .DefaultIfEmpty(Expression.Constant(false))
            .Aggregate(Expression.OrElse);

        return source.Where(Expression.Lambda<Func<DialogEntity, bool>>(predicate, dialogParameter));
    }
}
