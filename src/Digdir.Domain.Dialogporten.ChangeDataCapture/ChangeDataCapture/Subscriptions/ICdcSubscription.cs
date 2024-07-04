namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Subscriptions;

internal interface ICdcSubscription<T>
{
    IAsyncEnumerable<IReadOnlyCollection<T>> Subscribe(CancellationToken ct);
}
