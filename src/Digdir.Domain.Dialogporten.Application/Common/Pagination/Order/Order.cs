using Digdir.Domain.Dialogporten.Application.Common.Pagination.OrderOption;

namespace Digdir.Domain.Dialogporten.Application.Common.Pagination.Order;

public sealed class Order<TTarget>
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
