using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Digdir.Domain.Dialogporten.Application.Common.Pagination.Ordering;

public interface IOrderDefinition<TTarget>
{
    public static abstract IOrderOptions<TTarget> Configure(IOrderOptionsBuilder<TTarget> options);
}
public interface IOrderOptionsBuilder<TTarget>
{
    IOrderDefaultOptionsBuilder<TTarget> AddId(Expression<Func<TTarget, object?>> expression);
}

public interface IOrderDefaultOptionsBuilder<TTarget>
{
    IOrderOptionsOptionsBuilder<TTarget> AddDefault(string key, Expression<Func<TTarget, object?>> expression);
}


public interface IOrderOptionsOptionsBuilder<TTarget>
{
    IOrderOptionsOptionsBuilder<TTarget> AddOption(string key, Expression<Func<TTarget, object?>> expression);
    IOrderOptions<TTarget> Build();
}

internal class OrderOptionsBuilder<TTarget> : 
    IOrderOptionsBuilder<TTarget>, 
    IOrderDefaultOptionsBuilder<TTarget>, 
    IOrderOptionsOptionsBuilder<TTarget>
{
    private readonly Dictionary<string, OrderSelector<TTarget>> _optionByKey = new(StringComparer.InvariantCultureIgnoreCase);
    private string? _defaultKey;

    public IOrderDefaultOptionsBuilder<TTarget> AddId(Expression<Func<TTarget, object?>> expression)
    {
        _defaultKey = null;
        _optionByKey.Clear();
        _optionByKey[PaginationConstants.OrderIdKey] = new(expression);
        return this;
    }

    public IOrderOptionsOptionsBuilder<TTarget> AddDefault([NotNull] string key, Expression<Func<TTarget, object?>> expression)
    {
        _defaultKey = key;
        _optionByKey[_defaultKey] = new(expression);
        return this;
    }

    public IOrderOptionsOptionsBuilder<TTarget> AddOption([NotNull] string key, Expression<Func<TTarget, object?>> expression)
    {
        _optionByKey[key] = new(expression);
        return this;
    }

    internal OrderOptionsBuilder<TTarget> Configure<TOrderDefinition>()
        where TOrderDefinition : IOrderDefinition<TTarget>
    {
        TOrderDefinition.Configure(this);
        return this;
    }

    public IOrderOptions<TTarget> Build()
    {
        if (_defaultKey is null)
        {
            throw new InvalidOperationException("No default value is specified.");
        }

        return new OrderOptions<TTarget>(_defaultKey, _optionByKey);
    }
}

public interface IOrderOptions<TTarget>
{
    Order<TTarget> GetDefault();
    Order<TTarget> GetId();
    bool TryGetOption(string? key, [NotNullWhen(true)] out OrderSelector<TTarget>? option);
    bool TryParseOrder(string value, [NotNullWhen(true)] out Order<TTarget>? result);
}

public class OrderOptions<TTarget> : IOrderOptions<TTarget>
{
    private readonly string _defaultKey;
    private readonly Dictionary<string, OrderSelector<TTarget>> _optionByKey;

    public OrderOptions(string defaultKey, Dictionary<string, OrderSelector<TTarget>> optionByKey)
    {
        _defaultKey = defaultKey;
        _optionByKey = optionByKey;
    }

    public bool TryParseOrder(string value, [NotNullWhen(true)] out Order<TTarget>? result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        result = value.Split(PaginationConstants.OrderDelimiter, StringSplitOptions.TrimEntries) switch
        {
            // eks: createdAt
            [var key]
                when _optionByKey.TryGetValue(key, out var expression)
                => new(key, expression),

            // eks: createdAt_desc
            [var key, var direction]
                when _optionByKey.TryGetValue(key, out var expression)
                && Enum.TryParse<OrderDirection>(direction, ignoreCase: true, out var dirEnum)
                => new(key, expression, dirEnum),

            //// eks: createdAt_desc_continuationToken
            //[var key, var direction, var continuationToken]
            //    when _optionByKey.TryGetValue(key, out var expression)
            //    && Enum.TryParse<OrderDirection>(direction, out var dirEnum)
            //    && TryParseExtensions.TryParse(expression.CleanedBody.Type, continuationToken, out var ct)
            //    => Create(key, expression, dirEnum, ct),

            _ => null
        };

        return result is not null;
    }

    public Order<TTarget> GetId() => new(PaginationConstants.OrderIdKey, _optionByKey[PaginationConstants.OrderIdKey]);
    public Order<TTarget> GetDefault() => new(_defaultKey, _optionByKey[_defaultKey]);
    public bool TryGetOption(string? key, [NotNullWhen(true)] out OrderSelector<TTarget>? option)
    {
        option = default;
        return key is not null && _optionByKey.TryGetValue(key, out option);
    }
}

public record OrderSelector<TTarget>(Expression<Func<TTarget, object?>> Expression, Lazy<Func<TTarget, object?>> Compiled, Expression Body)
{
    public OrderSelector(Expression<Func<TTarget, object?>> expression) : this(expression, new(expression.Compile), RemoveConvertWrapper(expression.Body)) { }

    private static Expression RemoveConvertWrapper(Expression body) =>
        body.NodeType == ExpressionType.Convert && body is UnaryExpression unaryExpression
            ? unaryExpression.Operand
            : body;
}

public class Order<TTarget>
{
    private readonly OrderSelector<TTarget> _selector;

    public string Key { get; }
    public OrderDirection Direction { get; }

    public Order(string key, OrderSelector<TTarget> selector, OrderDirection direction = PaginationConstants.DefaultOrderDirection)
    {
        _selector = selector;
        Key = key;
        Direction = direction;
    }

    public OrderSelector<TTarget> GetSelector() => _selector;
}
