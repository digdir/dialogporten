namespace Digdir.Domain.Dialogporten.ChangeDataCapture.Common.Extensions;

internal static class AsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<TSource> ForEvery<TSource>(
        this IAsyncEnumerable<TSource> source,
        int threshold,
        Func<TSource, Task> action)
    {
        var counter = 0;
        await foreach (var item in source)
        {
            yield return item;

            if (++counter >= threshold)
            {
                await action(item);
                counter = 0;
            }
        }
    }
}
