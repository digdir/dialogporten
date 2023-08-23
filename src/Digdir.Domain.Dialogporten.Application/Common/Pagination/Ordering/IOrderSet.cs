using System.Diagnostics.CodeAnalysis;

namespace Digdir.Domain.Dialogporten.Application.Common.Pagination.Ordering;

public interface IOrderSet<TTarget>
{
    IReadOnlyCollection<IOrder<TTarget>> Orders { get; }
    string GetContinuationToken(TTarget t);
    string? GetOrderByAsString();
}

public sealed class OrderSet<TOrder, TTarget> : IOrderSet<TTarget>
    where TOrder : class, IParsableOrder<TOrder, TTarget>, new()
{
    public const char Delimiter = ',';

    private static readonly OrderSet<TOrder, TTarget> _default = 
        new() { Orders = new List<TOrder> { IParsableOrder<TOrder, TTarget>.Default, IParsableOrder<TOrder, TTarget>.Id } };
    public static OrderSet<TOrder, TTarget> Default => _default;

    public IReadOnlyCollection<TOrder> Orders { get; private set; } = null!;

    IReadOnlyCollection<IOrder<TTarget>> IOrderSet<TTarget>.Orders => Orders;

    public static bool TryParse(string? value, [NotNullWhen(true)] out OrderSet<TOrder, TTarget>? result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // eks: orderBy=createdAt_asc,updatedAt_desc
        var orders = new List<TOrder>();
        foreach (var orderAsString in value.Split(Delimiter, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!IParsableOrder<TOrder, TTarget>.TryParse(orderAsString, out var order))
            {
                return false;
            }

            orders.Add(order);
        }

        orders.Add(IDefaultOrder<TOrder, TTarget>.Id);

        result = new OrderSet<TOrder, TTarget> { Orders = orders };
        return true;
    }

    public string GetContinuationToken(TTarget t)
    {
        var values = Orders
            // TODO: cache?
            .Select(x => x.OrderBy.Compile().Invoke(t))
            .Select(x => x switch
            {
                null => string.Empty, 
                string s => s, 
                DateTimeOffset d => d.UtcDateTime.ToString("o"),
                DateTime d => d.ToString("o"),
                _ => x.ToString()
            })
            .ToArray();
        return string.Join(ContinuationToken.Delimiter, values);
    }

    public string? GetOrderByAsString()
    {
        var orderByString = string.Join(Delimiter, Orders
            .Take(..^1)
            .Where(x => !string.IsNullOrWhiteSpace(x.OrderByAsString))
            .Select(x => $"{x.OrderByAsString.ToLower()}{IParsableOrder<TOrder, TTarget>.Delimiter}{x.Direction.ToString().ToLower()}"));
        return string.IsNullOrWhiteSpace(orderByString) ? null : orderByString;
    }
}
