﻿using FluentValidation;
using FluentValidation.Internal;
using System.Linq.Expressions;

namespace Digdir.Domain.Dialogporten.Application.Common.Extensions.FluentValidation;

internal static class FluentValidation_Enumerable_Extensions
{
    public static IRuleBuilderOptions<T, IEnumerable<TProperty>> UniqueBy<T, TProperty, TKey>(
        this IRuleBuilder<T, IEnumerable<TProperty>> ruleBuilder,
        Func<TProperty, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null)
        where TProperty : class
    {
        return ruleBuilder.Must((parent, enumerable, ctx) =>
        {
            comparer ??= EqualityComparer<TKey>.Default;
            var duplicateKeys = enumerable
                .Select(keySelector)
                .Where(x => !Equals(x, default(TKey)))
                .GroupBy(x => x, comparer)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key)
                .ToArray();
            ctx.MessageFormatter.AppendArgument("DuplicateKeys", string.Join(",", duplicateKeys));
            return !duplicateKeys.Any();
        }).WithMessage("Can not contain duplicate items: [{DuplicateKeys}].");
    }

    public static IRuleBuilderOptions<T, TDependent> IsIn<T, TDependent, TPrincipal, TKey>(
        this IRuleBuilder<T, TDependent> ruleBuilder,
        Expression<Func<T, IEnumerable<TPrincipal>>> principalsSelector,
        Func<TDependent, TKey> dependentKeySelector,
        Func<TPrincipal, TKey> principalKeySelector,
        IEqualityComparer<TKey>? comparer = null)
    {
        return ruleBuilder.Must((parrent, dependent, ctx) =>
        {
            comparer ??= EqualityComparer<TKey>.Default;
            var dependentKey = dependentKeySelector(dependent);
            var member = principalsSelector.GetMember();
            var func = AccessorCache<T>.GetCachedAccessor(member, principalsSelector);
            var name = ValidatorOptions.Global.DisplayNameResolver(typeof(T), member, principalsSelector) ?? member.Name;
            ctx.MessageFormatter
                .AppendArgument("DependentKey", dependentKey)
                .AppendArgument("PrincipalName", name);
            return dependentKey is null ||
                func(parrent)
                    .Any(principal => comparer
                        .Equals(principalKeySelector(principal), dependentKey));
        })
        .WithMessage("Item '{DependentKey}' in '{PrincipalName}' does not exist.");
    }
}
