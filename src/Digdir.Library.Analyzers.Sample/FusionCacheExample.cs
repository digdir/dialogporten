using ZiggyCreatures.Caching.Fusion;
using System.Collections.ObjectModel;

namespace Digdir.Library.Analyzers.Sample;

public sealed class FusionCacheExample(IFusionCache fusionCache)
{
    public async Task<CachedStruct> GetCachedStruct(CancellationToken cancellationToken)
        => await fusionCache.GetOrSetAsync<CachedStruct>("Some key",
            CreateCachedStruct, token: cancellationToken);

    private static Task<CachedStruct> CreateCachedStruct(CancellationToken token) => Task.FromResult(new CachedStruct());

    public async Task<ReadOnlyCollection<CachedStruct>> GetCachedStructCollection(CancellationToken cancellationToken)
        => await fusionCache.GetOrSetAsync<ReadOnlyCollection<CachedStruct>>("Some key collection",
            CreateCachedStructCollection, token: cancellationToken);

    private static Task<ReadOnlyCollection<CachedStruct>> CreateCachedStructCollection(CancellationToken token)
        => Task.FromResult(new ReadOnlyCollection<CachedStruct>([]));

    public async Task<ReadOnlyDictionary<string, CachedStruct>> GetCachedStructDictionary(CancellationToken cancellationToken)
        => await fusionCache.GetOrSetAsync<ReadOnlyDictionary<string, CachedStruct>>("Some key dictionary",
            CreateCachedStructDictionary, token: cancellationToken);

    private static Task<ReadOnlyDictionary<string, CachedStruct>> CreateCachedStructDictionary(CancellationToken token)
        => Task.FromResult(new ReadOnlyDictionary<string, CachedStruct>(new Dictionary<string, CachedStruct>()));

    public async Task<CachedRecord> GetCachedRecord(CancellationToken cancellationToken)
        => await fusionCache.GetOrSetAsync<CachedRecord>("Some key record",
            CreateCachedRecord, token: cancellationToken);

    private static Task<CachedRecord> CreateCachedRecord(CancellationToken token) => Task.FromResult(new CachedRecord());

    public async Task<ReadOnlyCollection<CachedRecord>> GetCachedRecordCollection(CancellationToken cancellationToken)
        => await fusionCache.GetOrSetAsync<ReadOnlyCollection<CachedRecord>>("Some key record collection",
            CreateCachedRecordCollection, token: cancellationToken);

    private static Task<ReadOnlyCollection<CachedRecord>> CreateCachedRecordCollection(CancellationToken token)
        => Task.FromResult(new ReadOnlyCollection<CachedRecord>(new List<CachedRecord>()));

    public async Task<ReadOnlyDictionary<string, CachedRecord>> GetCachedRecordDictionary(CancellationToken cancellationToken)
        => await fusionCache.GetOrSetAsync<ReadOnlyDictionary<string, CachedRecord>>("Some key record dictionary",
            CreateCachedRecordDictionary, token: cancellationToken);

    private static Task<ReadOnlyDictionary<string, CachedRecord>> CreateCachedRecordDictionary(CancellationToken token)
        => Task.FromResult(new ReadOnlyDictionary<string, CachedRecord>(new Dictionary<string, CachedRecord>()));

    public async Task<CachedClass> GetCachedClass(CancellationToken cancellationToken)
        => await fusionCache.GetOrSetAsync<CachedClass>("Some key class",
            CreateCachedClass, token: cancellationToken);

    private static Task<CachedClass> CreateCachedClass(CancellationToken token) => Task.FromResult(new CachedClass());

    public async Task<ReadOnlyCollection<CachedClass>> GetCachedClassCollection(CancellationToken cancellationToken)
        => await fusionCache.GetOrSetAsync<ReadOnlyCollection<CachedClass>>("Some key class collection",
            CreateCachedClassCollection, token: cancellationToken);

    private static Task<ReadOnlyCollection<CachedClass>> CreateCachedClassCollection(CancellationToken token)
        => Task.FromResult(new ReadOnlyCollection<CachedClass>([]));

    public async Task<ReadOnlyDictionary<string, CachedClass>> GetCachedClassDictionary(CancellationToken cancellationToken)
        => await fusionCache.GetOrSetAsync<ReadOnlyDictionary<string, CachedClass>>("Some key class dictionary",
            CreateCachedClassDictionary, token: cancellationToken);

    private static Task<ReadOnlyDictionary<string, CachedClass>> CreateCachedClassDictionary(CancellationToken token)
        => Task.FromResult(new ReadOnlyDictionary<string, CachedClass>(new Dictionary<string, CachedClass>()));
}

public readonly struct CachedStruct;

public record CachedRecord;

public class CachedClass;
