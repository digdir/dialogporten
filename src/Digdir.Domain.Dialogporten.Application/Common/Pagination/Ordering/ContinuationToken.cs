using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Digdir.Domain.Dialogporten.Application.Common.Pagination.Ordering;

public class ContinuationToken
{
    public const char Delimiter = '_';

    private readonly List<string> _parts;

    public ContinuationToken(IEnumerable<string> parts)
    {
        _parts = new(parts ?? throw new ArgumentNullException(nameof(parts)));
    }

    public static bool TryParse(string? value, out ContinuationToken? result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var parts = value.Split(Delimiter, StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            return false;
        }

        result = new ContinuationToken(parts);
        return true;
    }

    public bool TryGetParts<TTarget>(IOrderSet<TTarget> orderSet, [NotNullWhen(true)] out IReadOnlyCollection<object?>? result) => 
        TryGetParts(orderSet.Orders, out result);

    public bool TryGetParts<TTarget>(IReadOnlyCollection<IOrder<TTarget>> orders, [NotNullWhen(true)] out IReadOnlyCollection<object?>? result)
    {
        if (orders is null)
        {
            throw new ArgumentNullException(nameof(orders));
        }
        if (orders.Count != _parts.Count)
        {
            // TODO: Throw exception? 
            throw new ArgumentException(
                $"The number of orders must match the number of parts in the continuation " +
                $"token. Got {orders.Count}, expected {_parts.Count}.", nameof(orders));
        }

        result = default;
        var typedParts = new List<object?>();
        foreach (var (part, order) in _parts.Zip(orders))
        {
            var type = order.OrderBy.Body.Type;
            if (order.OrderBy.Body.NodeType == ExpressionType.Convert 
                && order.OrderBy.Body is UnaryExpression unaryExpression)
            {
                type = unaryExpression.Operand.Type;
            }

            // TODO: Check string :P
            if (!TryParseExtensions.TryParse(type, part, out var parseResult))
            {
                return false;
            }

            typedParts.Add(parseResult);
        }

        result = typedParts;
        return true;
    }
}
