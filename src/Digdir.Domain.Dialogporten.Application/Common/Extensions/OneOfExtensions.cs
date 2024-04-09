using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Digdir.Domain.Dialogporten.Application.Common.Extensions;

internal static class OneOfExtensions
{
    private static readonly ConcurrentDictionary<(Type, Type), MethodInfo?> OneOfFactoryByType = new();

    public static bool TryConvertToOneOf<TOneOf>(object value, [NotNullWhen(true)] out TOneOf? result)
    {
        ArgumentNullException.ThrowIfNull(value);
        var oneOfFactory = OneOfFactoryByType.GetOrAdd(
            key: new(typeof(TOneOf), value.GetType()),
            valueFactory: GetOneOfFactoryOrNull);
        result = (TOneOf)oneOfFactory?.Invoke(null, [value])! ?? default;
        return oneOfFactory is not null;
    }

    private static MethodInfo? GetOneOfFactoryOrNull((Type, Type) arg)
    {
        const string implicitOperatorName = "op_Implicit";
        const BindingFlags implicitOperatorFlags = BindingFlags.Static | BindingFlags.Public;
        var (oneOf, type) = arg;
        var oneOfImplicitOperator = oneOf.GetMethod(
            name: implicitOperatorName,
            bindingAttr: implicitOperatorFlags,
            types: [type]);
        return oneOfImplicitOperator;
    }
}
