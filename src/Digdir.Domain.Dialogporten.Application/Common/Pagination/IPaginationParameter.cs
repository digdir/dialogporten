using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Common.Pagination;

public interface IPaginationParameter
{
    DateTimeOffset? After { get; }
    int? PageSize { get; }
}

public class DefaultPaginationParameter : IPaginationParameter
{
    private const int MaxPageSize = 1000;
    private const int DefaultPageSize = 100;
    private int _pageSize = DefaultPageSize;

    public DateTimeOffset? After { get; init; } = DateTimeOffset.MinValue;
    public int? PageSize
    {
        get => _pageSize;
        init => _pageSize = !value.HasValue 
            ? DefaultPageSize 
            : value.Value > MaxPageSize 
                ? MaxPageSize 
                : value.Value;
    }
}

internal sealed class PaginationParameterValidator : AbstractValidator<IPaginationParameter>
{
    public PaginationParameterValidator()
    {
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}
