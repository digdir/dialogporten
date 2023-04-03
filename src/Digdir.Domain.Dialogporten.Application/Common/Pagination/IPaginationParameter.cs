using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Common.Pagination;

public interface IPaginationParameter
{
    int PageIndex { get; set; }
    int PageSize { get; set; }
}

public class DefaultPaginationParameter : IPaginationParameter
{
    private const int MaxPageSize = 1000;
    private int _pageSize = 10;

    public int PageIndex { get; set; } = 0;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
}

internal sealed class PaginationParameterValidator : AbstractValidator<IPaginationParameter>
{
    public PaginationParameterValidator()
    {
        RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}
