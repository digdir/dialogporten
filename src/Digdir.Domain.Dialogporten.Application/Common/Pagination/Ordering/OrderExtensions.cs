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
        var idPart = orderParts[^1];
        orderParts = orderParts[..^1];

        var firstPart = orderParts
            .Select(x => x.orderDirection switch
            {
                OrderDirection.Asc => Expression.GreaterThan(x.orderBody, Expression.Constant(x.value, x.orderBody.Type)),
                OrderDirection.Desc => Expression.LessThan(x.orderBody, Expression.Constant(x.value, x.orderBody.Type)),
            })
            //.Aggregate(Expression.AndAlso);
            .Aggregate(Expression.OrElse);

        var secondPart = orderParts
            .Select(x => Expression.Equal(x.orderBody, Expression.Constant(x.value, x.orderBody.Type)))
            .Aggregate(Expression.AndAlso);
        var idCondition = idPart.orderDirection switch
        {
            OrderDirection.Asc => Expression.GreaterThan(idPart.orderBody, Expression.Constant(idPart.value, idPart.orderBody.Type)),
            OrderDirection.Desc => Expression.LessThan(idPart.orderBody, Expression.Constant(idPart.value, idPart.orderBody.Type)),
        };
        secondPart = Expression.AndAlso(secondPart, idCondition);

        var condition = Expression.OrElse(firstPart, secondPart);
        var queryPredicate = Expression.Lambda<Func<T, bool>>(condition, parameter);
        query = query.Where(queryPredicate);
        return query;
    }

    private static Expression CleanBody(this Expression body, ParameterReplacer? parameterReplacer)
    {
        if (body.NodeType == ExpressionType.Convert && body is UnaryExpression unaryExpression)
        {
            body = unaryExpression.Operand;
        }

        return parameterReplacer is not null 
            ? parameterReplacer.Visit(body)
            : body;
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