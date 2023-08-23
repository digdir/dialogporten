using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Digdir.Domain.Dialogporten.Application.Common.Pagination.Ordering;

public interface IOrder<TTarget>
{
    Expression<Func<TTarget, object?>> OrderBy { get; init; }
    OrderDirection Direction { get; init; }
}

public interface IDefaultOrder<TSelf, TTarget> : IOrder<TTarget>
    where TSelf : class, IDefaultOrder<TSelf, TTarget>, new()
{
    public const OrderDirection DefaultDirection = OrderDirection.Desc;
    private static readonly TSelf _default = new() { OrderBy = TSelf.GetDefaultOrderExpression(), Direction = DefaultDirection };
    private static readonly TSelf _id = new() { OrderBy = TSelf.GetIdExpression(), Direction = DefaultDirection };
    public static TSelf Default => _default;
    public static TSelf Id => _id;
    public abstract static Expression<Func<TTarget, object?>> GetIdExpression();
    public abstract static Expression<Func<TTarget, object?>> GetDefaultOrderExpression();
}

public interface IParsableOrder<TSelf, TTarget> : IDefaultOrder<TSelf, TTarget>
    where TSelf : class, IParsableOrder<TSelf, TTarget>, new()
{
    public const char Delimiter = '_';
    string OrderByAsString { get; init; }

    public static bool TryParse(string? value, [NotNullWhen(true)] out TSelf? result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        result = value.Split(Delimiter, StringSplitOptions.TrimEntries) switch
        {
            // eks: orderBy=createdAt
            [var byString] when
                TSelf.TryParseOrderExpression(byString, out var orderBy)
                => new() { OrderBy = orderBy, Direction = DefaultDirection, OrderByAsString = byString },
            // eks: orderBy=createdAt_asc
            [var byString, var directionString] when
                TSelf.TryParseOrderExpression(byString, out var orderBy) &&
                Enum.TryParse<OrderDirection>(directionString, ignoreCase: true, out var direction)
                => new() { OrderBy = orderBy, Direction = direction, OrderByAsString = byString },
            _ => null
        };

        return result is not null;
    }

    public abstract static bool TryParseOrderExpression(string? value, [NotNullWhen(true)] out Expression<Func<TTarget, object?>>? result);
}
