namespace Digdir.Domain.Dialogporten.Application.Common.Extensions.Enumerables;

public static class AsyncEnumerableExtensions
{
    public static IAsyncEnumerable<T> Empty<T>() => EmptyAsyncEnumerator<T>.Instance;

    private class EmptyAsyncEnumerator<T> : IAsyncEnumerable<T>, IAsyncEnumerator<T>
    {
        public static readonly EmptyAsyncEnumerator<T> Instance = new();
        public T Current => default!;
        public ValueTask DisposeAsync() => default;
        public ValueTask<bool> MoveNextAsync() => new(false);
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new()) => this;
    }
}
