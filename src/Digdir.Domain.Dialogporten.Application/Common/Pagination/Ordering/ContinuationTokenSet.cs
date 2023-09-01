using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Digdir.Domain.Dialogporten.Application.Common.Pagination.Ordering;

public class ContinuationToken
{
    public string Key { get; }
    public object? Value { get; }
    public ConstantExpression ValueExpression { get; }

    public ContinuationToken(string key, object? value, Type type)
    {
        Key = key;
        Value = value;
        ValueExpression = Expression.Constant(value, type);
    }
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

        var orderOptions = OrderSet<TOrderDefinition, TTarget>.OrderOptions;
        var tokens = new HashSet<ContinuationToken>(_tokenComparer);

        var continuationTokens = value
            .Split(PaginationConstants.ContinuationTokenSetDelimiter, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Split(PaginationConstants.ContinuationTokenDelimiter, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) switch
                {
                    [var key, var ctAsString]
                        when orderOptions.TryGetOption(key, out var option)
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