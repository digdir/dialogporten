using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace Digdir.Domain.Dialogporten.Application.Common.Pagination.Ordering;

public class ContinuationToken
{
    private static readonly MemberInfo ValueMemberInfo = typeof(ContinuationToken).GetProperty(nameof(Value))!;
    private readonly Type _type;
    public string Key { get; }
    public object? Value { get; }

    public ContinuationToken(string key, object? value, Type type)
    {
        Key = key;
        Value = value;
        _type = type;
    }

    public Expression GetValueExpression() => Expression.Convert(Expression.MakeMemberAccess(Expression.Constant(this), ValueMemberInfo), _type);
}

public interface IContinuationTokenSet
{
    public IReadOnlyCollection<ContinuationToken> Tokens { get; }
    public string Raw { get; }
}

public class ContinuationTokenSet<TOrderDefinition, TTarget> : IContinuationTokenSet
    where TOrderDefinition : IOrderDefinition<TTarget>
{
    private static readonly ContinuationTokenComparer _tokenComparer = new();

    public IReadOnlyCollection<ContinuationToken> Tokens { get; }
    public string Raw { get; }

    public ContinuationTokenSet(IReadOnlyCollection<ContinuationToken> tokens, string raw)
    {
        Tokens = tokens;
        Raw = raw;
    }

    public static bool TryParse(string? value, out ContinuationTokenSet<TOrderDefinition, TTarget>? result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var tokens = new HashSet<ContinuationToken>(_tokenComparer);
        var continuationTokens = value
            .Split(PaginationConstants.ContinuationTokenSetDelimiter, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Split(PaginationConstants.ContinuationTokenDelimiter, StringSplitOptions.TrimEntries) switch
                {
                    [var key, var ctAsString]
                        when OrderOptions<TOrderDefinition, TTarget>.Value.TryGetSelector(key, out var option)
                        && TryParseExtensions.TryParse(option.Body.Type, ctAsString, out var ct)
                        => new ContinuationToken(key, ct, option.Body.Type),
                    _ => null
                });

        foreach (var continuationToken in continuationTokens)
        {
            if (continuationToken is null || !tokens.Add(continuationToken))
            {
                return false;
            }
        }

        result = new(tokens, value);
        return true;
    }

    private class ContinuationTokenComparer : IEqualityComparer<ContinuationToken>
    {
        public bool Equals(ContinuationToken? x, ContinuationToken? y)
        {
            if (ReferenceEquals(x,y))
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            return x.Key == y.Key;
        }

        public int GetHashCode([DisallowNull] ContinuationToken obj)
        {
            return obj.Key.GetHashCode();
        }
    }
}