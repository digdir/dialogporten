using AutoMapper.Internal;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Digdir.Domain.Dialogporten.Application.Common.Pagination.Ordering;

public static class Parsing
{
    private static readonly ConcurrentDictionary<Type, MethodInfo?> _tryParseByType = new();

    public static bool TryParse(Type type, string? value, out object? result)
    {
        result = null;
        if (type.IsGenericType(typeof(Nullable<>)))
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }
            type = type.GetGenericArguments().Single();
        }
        var method = _tryParseByType.GetOrAdd(type, t => TryFindTryParseMethod(t, out var m) ? m : null);
        if (method is null)
        {
            return false;
        }

        var tryParseArguments = new object?[] { value, null };
        var tryParseResult = (bool)method.Invoke(null, tryParseArguments)!;
        result = tryParseArguments[1];
        return tryParseResult;
    }

    private static bool TryFindTryParseMethod(Type type, [NotNullWhen(true)] out MethodInfo? method)
    {
        const BindingFlags access = BindingFlags.Static | BindingFlags.Public;

        method = null;

        //find member of type with signature 'static public bool TryParse(string, out T)'
        var candidates = type.FindMembers(MemberTypes.Method,access,
            (m, _) => 
            {
                var method = (MethodInfo)m;
                if (method.Name != "TryParse") return false;
                if (method.ReturnParameter.ParameterType != typeof(bool)) return false;
                var parms = method.GetParameters();
                if (parms.Length != 2) return false;
                if (parms[0].ParameterType != typeof(string)) return false;
                if (parms[1].ParameterType != type.MakeByRefType()) return false;
                if (!parms[1].IsOut) return false;

                return true;

            }, null);

        if (candidates.Length == 1)
        {
            method = (MethodInfo)candidates[0];
        }

        return method is not null;
    }
}