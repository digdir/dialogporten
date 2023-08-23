using Digdir.Domain.Dialogporten.Application.Common.Pagination.Ordering;
using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Common.Pagination;

//public interface IPaginationParameter<TTarget>
public interface IPaginationParameter
{
    int? Limit { get; }
    ContinuationToken? Continue { get; }
    //IOrderSet<TTarget>? OrderBy { get; }
}

//public class DefaultPaginationParameter<TOrder, TTarget> : IPaginationParameter<TTarget>
//    where TOrder : class, IDefaultOrder<TOrder, TTarget>, new()
public class DefaultPaginationParameter : IPaginationParameter
{
    public const int MinLimit = 1;
    public const int MaxLimit = 1000;
    private const int DefaultLimit = 100;

    private int _limit = DefaultLimit;
    
    public ContinuationToken? Continue { get; init; }

    public int? Limit
    {
        get => _limit;
        init => _limit = value ?? DefaultLimit;
    }

    //IOrderSet<TTarget>? IPaginationParameter<TTarget>.OrderBy => OrderBy;
}

internal sealed class PaginationParameterValidator : AbstractValidator<IPaginationParameter>
{
    public PaginationParameterValidator()
    {
        RuleFor(x => x.Limit).InclusiveBetween(DefaultPaginationParameter.MinLimit, DefaultPaginationParameter.MaxLimit);
    }
}
