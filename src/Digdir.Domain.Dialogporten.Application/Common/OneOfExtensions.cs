using Digdir.Domain.Dialogporten.Application.Common.Exceptions;
using OneOf;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Digdir.Domain.Dialogporten.Application.Common;

internal static class OneOfExtensions
{
    private static readonly ConcurrentDictionary<(Type, Type), MethodInfo?> _oneOfFactoryByType = new();

    public static bool TryToOneOf<TOneOf>(object value, [NotNullWhen(true)] out TOneOf? result)
    {
        ArgumentNullException.ThrowIfNull(value);
        var oneOfFactory = _oneOfFactoryByType.GetOrAdd(
            key: new(typeof(TOneOf), value.GetType()),
            valueFactory: GetOneOfFactoryOrNull);
        result = (TOneOf)oneOfFactory?.Invoke(null, new object[] { value })! ?? default;
        return oneOfFactory is not null;
    }

    private static MethodInfo? GetOneOfFactoryOrNull((Type, Type) arg)
    {
        const string ImplicitOperatorName = "op_Implicit";
        const BindingFlags ImplicitOperatorFlags = BindingFlags.Static | BindingFlags.Public;
        var (duType, type) = arg;
        if (!duType.IsAssignableTo(typeof(IOneOf)) ||
            !duType.IsGenericType ||
            !duType.GetGenericArguments().Any(x => x == type))
        {
            return null;
        }

        var oneOfImplicitOperator = duType.GetMethod(
            name: ImplicitOperatorName,
            bindingAttr: ImplicitOperatorFlags,
            types: new[] { type });

        return oneOfImplicitOperator is not null 
            ? oneOfImplicitOperator 
            : throw new CriticalApplicationException(
                $"Could not find expected implicit operator on {duType} that accepts {type}. " +
                $"This may be due to a change in OneOf and must be fixed.");
    }
}
