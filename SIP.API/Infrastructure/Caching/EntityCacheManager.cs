using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System.Collections.Concurrent;

namespace SIP.API.Infrastructure.Caching;

public class EntityCacheManager(IMemoryCache cache)
{
    private readonly IMemoryCache _cache = cache;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _tokenSources = new();

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
        if (_tokenSources.TryGetValue(entityType, out var tokenSource))
        {
            try { tokenSource.Cancel(); } catch { /* swallow */ }
            try { tokenSource.Dispose(); } catch { /* swallow */ }
            _tokenSources[entityType] = new CancellationTokenSource();
        }
        else
        {
            _tokenSources[entityType] = new CancellationTokenSource();
        }
    }

    private CancellationTokenSource GetOrCreateTokenSource(string entityType) =>
        _tokenSources.GetOrAdd(entityType, _ => new CancellationTokenSource());

    /// <summary>
    /// Genérico: obtém do cache ou executa a factory assíncrona para preencher o cache.
    /// Use para qualquer tipo de valor; para contagens passe uma factory que retorne int.
    /// </summary>
    public async Task<T> GetOrSetAsync<T>(string cacheKey, Func<Task<T>> factory, string entityType, TimeSpan? expiration = null)
    {
        if (string.IsNullOrWhiteSpace(cacheKey)) throw new ArgumentException("cacheKey é obrigatório.", nameof(cacheKey));
        if (factory is null) throw new ArgumentNullException(nameof(factory));

        if (_cache.TryGetValue(cacheKey, out T? cached) && cached is not null)
            return cached;

        T value = await factory();
        Set(cacheKey, value, entityType, expiration);
        return value;
    }

    /// <summary>
    /// Especialização para contagens (int) — conveniência para services.
    /// </summary>
    public Task<int> GetOrSetCountAsync(string cacheKey, Func<Task<int>> countFactory, string entityType, TimeSpan? expiration = null) =>
        GetOrSetAsync<int>(cacheKey, countFactory, entityType, expiration);
}