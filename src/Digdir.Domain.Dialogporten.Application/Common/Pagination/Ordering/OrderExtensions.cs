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
            .Zip(orderSet.Orders, (part, order) => (value: part, orderDirection: order.Direction, orderBody: order.OrderBy.Body.CleanBody(parameterReplacer)))
            .ToArray();

        // ltGtParts =      [ a < an, b < bn, c < cn, id < idn ]
        // equalParst =     [ a = an, b = bn, c = cn, id = idn ]
        // intermediate =   [ a < an, a = an AND b < bn, a = an AND b = bn AND c < cn ]
        // condition =      a < an OR (a = an AND b < bn) OR (a = an AND b = bn AND c < cn) OR ...
        var ltGtParts = orderParts
            .Select(x => x.orderDirection switch
            {
                OrderDirection.Asc => Expression.GreaterThan(x.orderBody, Expression.Constant(x.value, x.orderBody.Type)),
                OrderDirection.Desc => Expression.LessThan(x.orderBody, Expression.Constant(x.value, x.orderBody.Type)),
            })
            .ToArray();

        var eqParts = orderParts
            .Select(x => Expression.Equal(x.orderBody, Expression.Constant(x.value, x.orderBody.Type)))
            .ToArray();

        var condition = ltGtParts
            .Select((ltGtPart, index) => eqParts[..index]
                .Concat(new[] { ltGtPart })
                .Aggregate(Expression.AndAlso))
            .Aggregate(Expression.OrElse);

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
