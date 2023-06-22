using Digdir.Domain.Dialogporten.Application.Common.Numbers;
using FluentValidation;
using FluentValidation.Internal;
using System.Linq.Expressions;

namespace Digdir.Domain.Dialogporten.Application.Common.Extensions;

internal static class FluentValidationExtensions
{
    public static IRuleBuilderOptions<T, TUri> IsValidUri<T, TUri>(this IRuleBuilder<T, TUri> ruleBuilder)
        where TUri : Uri?
    {
        return ruleBuilder
            .Must(uri => uri is null || uri.IsWellFormedOriginalString())
            .WithMessage("'{PropertyName}' is not a well formated URI.");
    }

    public static IRuleBuilderOptions<T, TGuid> MaximumLength<T, TGuid>(this IRuleBuilder<T, TGuid> ruleBuilder, int maximumLength)
        where TGuid : Uri?
    {
        return ruleBuilder
            .Must((_, uri, context) =>
            {
                var length = uri?.ToString().Length;
                context.MessageFormatter
                    .AppendArgument("MaxLength", maximumLength)
                    .AppendArgument("TotalLength", length);
                return !length.HasValue || length <= maximumLength;
            })
            .WithMessage("The length of '{PropertyName}' must be {MaxLength} characters or fewer. You entered {TotalLength} characters.");
    }

    public static IRuleBuilderOptions<T, Guid?> IsValidUuidV7<T>(this IRuleBuilder<T, Guid?> ruleBuilder)
    {
        return ruleBuilder
            .Must(x => !x.HasValue || UuidV7.IsValid(x.Value))
            .WithMessage("'{PropertyName}' is not a valid v7 uuid.");
    }

    public static IRuleBuilderOptions<T, Guid> IsValidUuidV7<T>(this IRuleBuilder<T, Guid> ruleBuilder)    
    {
        return ruleBuilder
            .Must(UuidV7.IsValid)
            .WithMessage("'{PropertyName}' is not a valid v7 uuid.");
    }

    public static IRuleBuilderOptions<T, IEnumerable<TProperty>> UniqueBy<T, TProperty, TKey>(
        this IRuleBuilder<T, IEnumerable<TProperty>> ruleBuilder,
        Func<TProperty, TKey> keySelector)
        where TProperty : class
    {
        return ruleBuilder.Must((parent, enumerable, ctx) =>
        {
            var duplicateKeys = enumerable
                .GroupBy(keySelector)
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
        EqualityComparer<TKey>? equalityComparer = null)
    {
        return ruleBuilder.Must((parrent, dependent, ctx) =>
        {
            equalityComparer ??= EqualityComparer<TKey>.Default;
            var compiledPrincipalSelector = principalsSelector.Compile();
            var principalName = principalsSelector.GetMember().Name;
            var dependentKey = dependentKeySelector(dependent);
            ctx.MessageFormatter
                .AppendArgument("DependentKey", dependentKey)
                .AppendArgument("PrincipalName", principalName);
            return dependentKey is null ||
                compiledPrincipalSelector(parrent)
                    .Any(principal => equalityComparer
                        .Equals(principalKeySelector(principal), dependentKey));
        })
        .WithMessage("Item '{DependentKey}' in '{PrincipalName}' does not exist.");
    }
}
