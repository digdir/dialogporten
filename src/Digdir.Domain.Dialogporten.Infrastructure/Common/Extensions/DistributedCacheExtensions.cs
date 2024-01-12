using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Digdir.Domain.Dialogporten.Infrastructure.Common.Extensions;

internal static class DistributedCacheExtensions
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    /// <summary>
    /// Gets the value associated with the specified key from the distributed cache, or adds it if not found.
    /// </summary>
    /// <typeparam name="T">The type of value to retrieve or add.</typeparam>
    /// <param name="cache">The distributed cache instance.</param>
    /// <param name="key">The cache key to look up or add.</param>
    /// <param name="valueFactory">
    /// A function that returns the value to be added to the cache if not found.
    /// This function takes a CancellationToken as a parameter for asynchronous operations.
    /// </param>
    /// <param name="optionsFactory">
    /// An optional function that provides cache entry options based on the generated value.
    /// If not provided or the function returns null, default options will be used.
    /// </param>
    /// <param name="jsonOptions">
    /// Optional. The JSON serialization options to use when serializing and deserializing the cache value.
    /// </param>
    /// <param name="cancellationToken">A CancellationToken that can be used to cancel the operation.</param>
    /// <returns>
    /// The value associated with the specified key, either retrieved from the cache or added using the provided factory function.
    /// If the value cannot be found in the cache or if the specified options indicate that the cached value has expired,
    /// the provided factory function's result will be returned.
    /// </returns>
    /// <remarks>
    /// This method first attempts to retrieve the value associated with the given key from the distributed cache.
    /// If the value is found, it is deserialized and returned. If not found or if the cache entry options indicate that the
    /// cached value has expired, the provided valueFactory function is called to generate the value. The optionsFactory
    /// function can be used to specify custom cache entry options based on the generated value, such as setting an absolute
    /// expiration time or using sliding expiration.
    /// </remarks>
    internal static async Task<T> GetOrAddAsync<T>(
        this IDistributedCache cache,
        string key,
        Func<CancellationToken, Task<T>> valueFactory,
        Func<T, DistributedCacheEntryOptions?>? optionsFactory = null,
        JsonSerializerOptions? jsonOptions = null,
        CancellationToken cancellationToken = default)
    {
        var value = await TryGetFromCache<T>(cache, key, jsonOptions, cancellationToken);
        if (value is not null)
        {
            return value;
        }

        var keyLock = _locks.GetOrAdd(key, new SemaphoreSlim(1, 1));
        await keyLock.WaitAsync(cancellationToken);

        try
        {
            // Try getting the value from cache again (it might have been added to cache by another task while waiting for lock).
            value = await TryGetFromCache<T>(cache, key, jsonOptions, cancellationToken);
            if (value is not null)
            {
                return value;
            }

            value = await valueFactory(cancellationToken);
            var options = optionsFactory?.Invoke(value) ?? new();
            if (options.AbsoluteExpiration < DateTimeOffset.UtcNow)
            {
                return value;
            }

            await cache.SetAsync(key, JsonSerializer.SerializeToUtf8Bytes(value), options, cancellationToken);
        }
        finally
        {
            keyLock.Release();
        }
        return value;
    }

    private static async Task<T?> TryGetFromCache<T>(IDistributedCache cache, string key, JsonSerializerOptions? jsonOptions, CancellationToken cancellationToken)
    {
        var cachedValue = await cache.GetAsync(key, cancellationToken);
        if (cachedValue is not null)
        {
            return JsonSerializer.Deserialize<T>(cachedValue, jsonOptions);
        }
        return default;
    }
}
