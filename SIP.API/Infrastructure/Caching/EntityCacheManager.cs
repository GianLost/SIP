using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace SIP.API.Infrastructure.Caching;

public class EntityCacheManager(IMemoryCache cache)
{
    private readonly IMemoryCache _cache = cache;
    private readonly Dictionary<string, CancellationTokenSource> _tokenSources = [];

    public T? Get<T>(string key) => 
       _cache.TryGetValue(key, out T? value) ? value : default;

    public void Set<T>(string key, T value, string entityType, TimeSpan? expiration = null)
    {
        CancellationTokenSource tokenSource = GetOrCreateTokenSource(entityType);
        MemoryCacheEntryOptions options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(expiration ?? TimeSpan.FromMinutes(2))
            .AddExpirationToken(new CancellationChangeToken(tokenSource.Token));

        _cache.Set(key, value, options);
    }

    public void Invalidate(string entityType)
    {
        if (_tokenSources.TryGetValue(entityType, out CancellationTokenSource? tokenSource))
        {
            tokenSource.Cancel();
            tokenSource.Dispose();
            _tokenSources[entityType] = new CancellationTokenSource();
        }
        else
        {
            _tokenSources[entityType] = new CancellationTokenSource();
        }
    }

    private CancellationTokenSource GetOrCreateTokenSource(string entityType)
    {
        if (!_tokenSources.TryGetValue(entityType, out CancellationTokenSource? tokenSource))
        {
            tokenSource = new CancellationTokenSource();
            _tokenSources[entityType] = tokenSource;
        }
        return tokenSource;
    }
}