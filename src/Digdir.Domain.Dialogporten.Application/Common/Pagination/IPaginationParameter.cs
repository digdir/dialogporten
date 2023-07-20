using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Common.Pagination;

public interface IPaginationParameter
{
    int? PageIndex { get; set; }
    int? PageSize { get; set; }
}

public class DefaultPaginationParameter : IPaginationParameter
{
    private const int MaxPageSize = 1000;
    private const int DefaultPageSize = 100;
    private int _pageSize = DefaultPageSize;

    public int? PageIndex { get; set; } = 0;
    public int? PageSize
    {
        get => _pageSize;
        set => _pageSize = !value.HasValue 
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
        RuleFor(x => x.PageIndex).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1);
    }
}
