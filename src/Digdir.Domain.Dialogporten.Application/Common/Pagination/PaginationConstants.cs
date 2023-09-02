using Digdir.Domain.Dialogporten.Application.Common.Pagination.Ordering;

namespace Digdir.Domain.Dialogporten.Application.Common.Pagination;

internal static class PaginationConstants
{
    public const int MinLimit = 1;
    public const int MaxLimit = 1000;
    public const int DefaultLimit = 100;

    public const char OrderSetDelimiter = ',';
    public const char OrderDelimiter = '_';
    public const string OrderIdKey = "id";

    public const char ContinuationTokenSetDelimiter = ',';
    public const char ContinuationTokenDelimiter = '_';

    public const OrderDirection DefaultOrderDirection = OrderDirection.Desc;
}