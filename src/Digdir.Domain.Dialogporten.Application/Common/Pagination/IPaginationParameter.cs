using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Common.Pagination;

public interface IPaginationParameter
{
    DateTimeOffset? Continue { get; }
    int? Limit { get; }
    OrderDirection? Direction { get; }
}

public class DefaultPaginationParameter : IPaginationParameter
{
    private const int MaxLimit = 1000;
    private const int DefaultLimit = 100;
    private const OrderDirection DefaultOrderDirection = OrderDirection.Desc;

    private int _limit = DefaultLimit;
    private DateTimeOffset _continue = DateTimeOffset.MaxValue;
    private OrderDirection _direction = DefaultOrderDirection;

    public OrderDirection? Direction 
    { 
        get => _direction; 
        init 
        {
            _direction = value ?? DefaultOrderDirection;
            if (value == OrderDirection.Asc && _continue == DateTimeOffset.MaxValue)
            {
                _continue = DateTimeOffset.MinValue;
            }
        } 
    }

    public DateTimeOffset? Continue
    {
        get => _continue;
        init => _continue = value ?? _direction switch
        {
            OrderDirection.Asc => DateTimeOffset.MinValue,
            OrderDirection.Desc => DateTimeOffset.MaxValue,
            _ => throw new InvalidOperationException($"{nameof(Direction)} is not a valid {nameof(OrderDirection)}. Got {_direction}."),
        };
    }

    public int? Limit
    {
        get => _limit;
        init => _limit = value ?? DefaultLimit;
    }
}

internal sealed class PaginationParameterValidator : AbstractValidator<IPaginationParameter>
{
    public PaginationParameterValidator()
    {
        RuleFor(x => x.Limit).InclusiveBetween(1, 1000);
        RuleFor(x => x.Direction).IsInEnum();
    }
}
