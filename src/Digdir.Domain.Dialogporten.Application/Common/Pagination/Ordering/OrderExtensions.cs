using System.Linq.Expressions;

namespace Digdir.Domain.Dialogporten.Application.Common.Pagination.Ordering;

internal static class OrderExtensions
{
    public static OrderSet<TOrder, TTarget> DefaultIfNull<TOrder, TTarget>(this OrderSet<TOrder, TTarget>? orderSet)
        where TOrder : class, IParsableOrder<TOrder, TTarget>, new()
        where TTarget : class
    {
        return orderSet ?? OrderSet<TOrder, TTarget>.Default;
    }

    public static IQueryable<T> ApplyOrder<T>(this IQueryable<T> query, IOrderSet<T> orderSet)
    {
        var first = true;

        foreach (var order in orderSet.Orders)
        {
            query = query.ApplyOrder(order, first);
            first = false;
        }

        return query;
    }

    private static IQueryable<T> ApplyOrder<T>(this IQueryable<T> query, IOrder<T> order, bool first)
    {
        if (first)
        {
            return order.Direction switch
            {
                OrderDirection.Asc => query.OrderBy(order.OrderBy),
                OrderDirection.Desc => query.OrderByDescending(order.OrderBy),
                _ => throw new ArgumentOutOfRangeException(nameof(order.Direction), order.Direction, null)
            };
        }
        return order.Direction switch
        {
            OrderDirection.Asc => ((IOrderedQueryable<T>)query).ThenBy(order.OrderBy),
            OrderDirection.Desc => ((IOrderedQueryable<T>)query).ThenByDescending(order.OrderBy),
            _ => throw new ArgumentOutOfRangeException(nameof(order.Direction), order.Direction, null)
        };
    }

    public static IQueryable<T> ApplyCondition<T>(this IQueryable<T> query, IOrderSet<T> orderSet, ContinuationToken? continuationToken)
    {
        if (continuationToken is null)
        {
            return query;
        }

        if (!continuationToken.TryGetParts(orderSet, out var parts))
        {
            // TODO: Handle invalid continuation token
            throw new Exception("Invalid continuation token.");
        }

        var parameter = Expression.Parameter(typeof(T));
        var parameterReplacer = new ParameterReplacer(parameter);

        var orderParts = parts
            .Zip(orderSet.Orders, (part, order) =>
            {
                var orderBody = order.OrderBy.Body.CleanBody(parameterReplacer);
                var orderDirection = order.Direction;
                var valueExpression = Expression.Constant(part, orderBody.Type);
                return (orderBody, orderDirection, valueExpression);
            })
            .ToArray();

        // The following is the algorithm for creating the condition expression.
        // The idea is to create a condition expression that looks like this:
        // x => x.a < an OR (x.a = an AND x.b < bn) OR (x.a = an AND x.b = bn AND x.c < cn) OR ...
        // where a, b, c, ... are the order by expressions and an, bn, cn, ...
        // are the values from the continuation token. It's a repeating pattern
        // where the previous properties are equal and the next property is less than
        // or greater than the next value, depending on the sort order. 
        // See https://phauer.com/2018/web-api-pagination-timestamp-id-continuation-token/ for more info.
        // The algorithm is as follows: 
        // equalParst =         [ a = an, b = bn, c = cn, ... ]
        // ltGtParts =          [ a < an, b < bn, c < cn, ... ]
        // lgGtEqualParst =     [ a < an, a = an AND b < bn, a = an AND b = bn AND c < cn, ... ]
        // condition =          a < an OR (a = an AND b < bn) OR (a = an AND b = bn AND c < cn) OR ...
        // qeryPredicate =      x => x.a < an OR (x.a = an AND x.b < bn) OR (x.a = an AND x.b = bn AND x.c < cn) OR ...

        // [ a = an, b = bn, c = cn, ... ]
        var eqParts = orderParts
            .Select(x => Expression.Equal(x.orderBody, x.valueExpression))
            .ToArray();

        var condition = orderParts
            // [ a < an, b < bn, c < cn, ... ]
            .Select(x => x.orderDirection switch
            {
                OrderDirection.Asc => Expression.GreaterThan(x.orderBody, x.valueExpression),
                OrderDirection.Desc => Expression.LessThan(x.orderBody, x.valueExpression),
            })
            // [ a < an, a = an AND b < bn, a = an AND b = bn AND c < cn, ... ]
            .Select((ltGtPart, index) => eqParts[..index]
                .Append(ltGtPart)
                .Aggregate(Expression.AndAlso))
            // a < an OR (a = an AND b < bn) OR (a = an AND b = bn AND c < cn) OR ...
            .Aggregate(Expression.OrElse);

        // x => x.a < an OR (x.a = an AND x.b < bn) OR (x.a = an AND x.b = bn AND x.c < cn) OR ...
        var queryPredicate = Expression.Lambda<Func<T, bool>>(condition, parameter);
        query = query.Where(queryPredicate);
        return query;
    }

    private static Expression CleanBody(this Expression body, ParameterReplacer parameterReplacer)
    {
        if (body.NodeType == ExpressionType.Convert && body is UnaryExpression unaryExpression)
        {
            body = unaryExpression.Operand;
        }

        return parameterReplacer.Visit(body);
    }

    private class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _newParameter;

        public ParameterReplacer(ParameterExpression newParameter)
        {
            _newParameter = newParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _newParameter;
        }
    }
}
