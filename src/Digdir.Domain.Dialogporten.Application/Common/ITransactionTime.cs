namespace Digdir.Domain.Dialogporten.Application.Common;

/// <summary>
/// Abstraction around the time that is to be used for all time
/// related DB and event fields within the same transaction.
/// </summary>
public interface ITransactionTime
{
    DateTimeOffset Value { get; }
}

/// <summary>
/// This will wait to the last possible moment to get the
/// current time and use that as the transaction time.
/// </summary>
internal sealed class TransactionTime : ITransactionTime
{
    private readonly Lazy<DateTimeOffset> _lazyValue;
    public DateTimeOffset Value => _lazyValue.Value;
    public TransactionTime(IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        _lazyValue = new(() => clock.UtcNow);
    }
}
