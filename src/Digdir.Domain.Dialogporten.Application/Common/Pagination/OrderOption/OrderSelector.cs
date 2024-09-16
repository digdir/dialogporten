using System.Linq.Expressions;

namespace Digdir.Domain.Dialogporten.Application.Common.Pagination.OrderOption;

public sealed record OrderSelector<TTarget>(Expression<Func<TTarget, object?>> Expression, Lazy<Func<TTarget, object?>> Compiled, Expression Body)
{
    public OrderSelector(Expression<Func<TTarget, object?>> expression) : this(expression, new(expression.Compile), RemoveConvertWrapper(expression.Body)) { }

    private static Expression RemoveConvertWrapper(Expression body) =>
        body.NodeType == ExpressionType.Convert && body is UnaryExpression unaryExpression
            ? unaryExpression.Operand
            : body;
}
