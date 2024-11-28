using ZiggyCreatures.Caching.Fusion;

namespace Digdir.Library.Analyzers.Sample;

public sealed class FusionCacheExample(IFusionCache fusionCache)
{
    public async Task<TheCachedThing> GetCachedValue(CancellationToken cancellationToken)
    {
        var authorizedParties = await fusionCache.GetOrSetAsync<TheCachedThing>("Some key",
            GetValueToPutInCache, token: cancellationToken);

        return authorizedParties;
    }


    private static async Task<TheCachedThing> GetValueToPutInCache(CancellationToken token)
    {
        await Task.Delay(0, token);
        return new TheCachedThing();
    }
}

public readonly struct TheCachedThing;
