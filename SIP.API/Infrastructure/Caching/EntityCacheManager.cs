using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System.Collections.Concurrent;

namespace SIP.API.Infrastructure.Caching;

/// <summary>
/// Provides a strongly-typed, thread-safe caching mechanism for application entities.
/// </summary>
/// <remarks>
/// The <see cref="EntityCacheManager"/> acts as a generic abstraction over
/// <see cref="IMemoryCache"/>, offering entity-based cache segmentation and
/// automatic invalidation via <see cref="CancellationTokenSource"/> tokens.
///
/// This design allows all cached data associated with a specific entity type
/// (e.g., <c>User</c>, <c>Sector</c>) to be invalidated in a single operation,
/// ensuring data consistency across services.
///
/// Typical usage:
/// <code>
/// // Example: caching total user count
/// int totalCount = await _cache.GetOrSetCountAsync(
///     cacheKey: $"{CacheKeys.UsersTotalCount}",
///     countFactory: () => _context.Users.CountAsync(),
///     entityType: "User"
/// );
///
/// // When users are modified:
/// _cache.Invalidate("User");
/// </code>
/// </remarks>
public class EntityCacheManager(IMemoryCache cache)
{
    private readonly IMemoryCache _cache = cache;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _tokenSources = new();

    /// <summary>
    /// Retrieves an item from the cache by its key.
    /// </summary>
    /// <typeparam name="T">The expected type of the cached value.</typeparam>
    /// <param name="key">The unique cache key.</param>
    /// <returns>
    /// The cached value if present; otherwise, the default value for <typeparamref name="T"/>.
    /// </returns>
    /// <remarks>
    /// This method does not throw exceptions if the key does not exist.
    /// It is a simple convenience wrapper over <see cref="IMemoryCache.TryGetValue"/>.
    /// </remarks>
    public T? Get<T>(string key) =>
        _cache.TryGetValue(key, out T? value) ? value : default;

    /// <summary>
    /// Adds or updates a cached entry for the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the value being cached.</typeparam>
    /// <param name="key">The unique key that identifies the cached entry.</param>
    /// <param name="value">The value to be stored in the cache.</param>
    /// <param name="entityType">
    /// The logical entity category associated with this cache entry (e.g., "User", "Sector").
    /// Used for group invalidation.
    /// </param>
    /// <param name="expiration">
    /// Optional expiration duration. If omitted, defaults to 2 minutes.
    /// </param>
    /// <remarks>
    /// Each entity type is associated with a unique <see cref="CancellationTokenSource"/>.  
    /// When <see cref="Invalidate(string)"/> is called, all entries that share the same entity type
    /// token are automatically invalidated.
    /// </remarks>
    public void Set<T>(string key, T value, string entityType, TimeSpan? expiration = null)
    {
        CancellationTokenSource tokenSource = GetOrCreateTokenSource(entityType);
        MemoryCacheEntryOptions options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(expiration ?? TimeSpan.FromMinutes(2))
            .AddExpirationToken(new CancellationChangeToken(tokenSource.Token));

        _cache.Set(key, value, options);
    }

    /// <summary>
    /// Invalidates all cached entries associated with the specified entity type.
    /// </summary>
    /// <param name="entityType">
    /// The entity category whose cache entries should be invalidated.
    /// </param>
    /// <remarks>
    /// This operation cancels the current <see cref="CancellationTokenSource"/> for the
    /// entity type and replaces it with a new one.  
    /// All entries tied to the old token will expire immediately.
    /// </remarks>
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

    /// <summary>
    /// Retrieves a cached value or executes an asynchronous factory function to populate the cache.
    /// </summary>
    /// <typeparam name="T">The type of value to retrieve or store.</typeparam>
    /// <param name="cacheKey">The unique key identifying the cached item.</param>
    /// <param name="factory">
    /// An asynchronous delegate that produces the value if it is not already cached.
    /// </param>
    /// <param name="entityType">
    /// The logical entity type associated with this cache entry.
    /// Used for token-based invalidation.
    /// </param>
    /// <param name="expiration">
    /// Optional expiration duration. Defaults to 2 minutes if not provided.
    /// </param>
    /// <returns>
    /// The cached value if available, otherwise the result of the factory function (which will be cached).
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="cacheKey"/> is null or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="factory"/> is null.
    /// </exception>
    /// <remarks>
    /// This method is generic and can be used for any type of cached data — not only counts.
    /// It provides a convenient way to implement *lazy caching* patterns throughout services.
    /// </remarks>
    public async Task<T> GetOrSetAsync<T>(
        string cacheKey, Func<Task<T>> factory, 
        string entityType, 
        TimeSpan? expiration = null)
    {
        if (string.IsNullOrWhiteSpace(cacheKey)) throw new ArgumentException("cacheKey é obrigatório.", nameof(cacheKey));

        ArgumentNullException.ThrowIfNull(factory);

        if (_cache.TryGetValue(cacheKey, out T? cached) && cached is not null)
            return cached;

        T value = await factory();

        Set(cacheKey, value, entityType, expiration);

        return value;
    }

    /// <summary>
    /// Retrieves or caches an integer count (specialized convenience method).
    /// </summary>
    /// <param name="cacheKey">The unique cache key for the count value.</param>
    /// <param name="countFactory">
    /// A function that asynchronously computes the count when the cache is empty or expired.
    /// </param>
    /// <param name="entityType">
    /// The entity type used for cache invalidation (e.g., "User", "Sector").
    /// </param>
    /// <param name="expiration">
    /// Optional cache duration; defaults to 2 minutes.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation, containing the count value.
    /// </returns>
    /// <remarks>
    /// This is a convenience wrapper around <see cref="GetOrSetAsync{T}"/>,
    /// preconfigured for <see cref="int"/> values.
    /// It simplifies caching of total record counts commonly used in pagination services.
    /// </remarks>
    public Task<int> GetOrSetCountAsync(
        string cacheKey, 
        Func<Task<int>> countFactory, 
        string entityType, 
        TimeSpan? expiration = null) =>
            GetOrSetAsync<int>(cacheKey, countFactory, entityType, expiration);

    public static string BuildScopedCacheKey(
        string baseKey, 
        string? search = null, 
        Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(baseKey))
            throw new ArgumentException("baseKey é obrigatório.", nameof(baseKey));

        if (id.HasValue)
            return $"{baseKey}:id:{id.Value}";

        if (!string.IsNullOrWhiteSpace(search))
            return $"{baseKey}:search:{Uri.EscapeDataString(search.Trim().ToLowerInvariant())}";

        return $"{baseKey}:all";
    }

    /// <summary>
    /// Retrieves or creates a <see cref="CancellationTokenSource"/> for a given entity type.
    /// </summary>
    /// <param name="entityType">The entity type identifier.</param>
    /// <returns>
    /// A <see cref="CancellationTokenSource"/> used for invalidation of all
    /// cache entries related to the given entity type.
    /// </returns>
    private CancellationTokenSource GetOrCreateTokenSource(string entityType) =>
        _tokenSources.GetOrAdd(entityType, _ => new CancellationTokenSource());
}