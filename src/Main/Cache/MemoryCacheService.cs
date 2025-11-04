using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace BaseNetCore.Core.src.Main.Cache
{
    public class MemoryCacheService : ICacheService
    {
        public readonly IMemoryCache _memoryCache;
        private readonly ILogger<MemoryCacheService> _logger;
        private readonly ConcurrentDictionary<string, byte> _keyTracker = new();
        private static readonly MemoryCacheEntryOptions _defaultCacheOptions = new MemoryCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        };
        public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }



        public Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                if (_memoryCache.TryGetValue(key, out T? value))
                {
                    _logger.LogDebug("Cache hit for key: {Key}", key);
                    return Task.FromResult(value);
                }

                _logger.LogDebug("Cache miss for key: {Key}", key);
                return Task.FromResult<T?>(null);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting cache for key: {Key}", key);
                return Task.FromResult<T?>(null);
            }
        }
        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            try
            {
                var options = expiration.HasValue
                    ? new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration }
                    : _defaultCacheOptions;

                options.RegisterPostEvictionCallback((k, v, reason, state) =>
                {
                    _keyTracker.TryRemove(k.ToString()!, out _);
                    _logger.LogDebug("Cache entry for key: {Key} evicted due to {Reason}", k, reason);
                });
                _memoryCache.Set(key, value, options);
                _keyTracker.TryAdd(key, 0);
                _logger.LogDebug("Cache set for key: {Key} with expiration: {Expiration}", key, expiration);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error setting cache for key: {Key}", key);
            }
            return Task.CompletedTask;

        }

        public Task RemoveAsync(string key)
        {
            try
            {
                _memoryCache.Remove(key);
                _keyTracker.TryRemove(key, out _);
                _logger.LogDebug("Cache removed for key: {Key}", key);

            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error removing cache for key: {Key}", key);
            }
            return Task.CompletedTask;
        }

        public Task RemoveByPatternAsync(string pattern)
        {
            try
            {
                var keysToRemove = _keyTracker.Keys
                    .Where(k => k.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    _memoryCache.Remove(key);
                    _keyTracker.TryRemove(key, out _);
                }

                _logger.LogDebug("Removed {Count} cache entries matching pattern: {Pattern}",
                    keysToRemove.Count, pattern);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error removing cache by pattern: {Pattern}", pattern);
            }

            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key)
        {
            return Task.FromResult(_memoryCache.TryGetValue(key, out _));
        }


    }
}
