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

    public static IRuleBuilderOptions<T, TUri> MaximumLength<T, TUri>(this IRuleBuilder<T, TUri> ruleBuilder, int maximumLength)
        where TUri : Uri?
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