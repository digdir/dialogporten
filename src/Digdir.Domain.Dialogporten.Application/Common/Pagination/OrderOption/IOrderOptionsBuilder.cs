using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Digdir.Domain.Dialogporten.Application.Common.Pagination.OrderOption;

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

    public IOrderOptions<TTarget> Build()
    {
        if (_defaultKey is null)
        {
            throw new InvalidOperationException("No default value is specified.");
        }

        return new OrderOptions<TTarget>(_defaultKey, _optionByKey);
    }
}
