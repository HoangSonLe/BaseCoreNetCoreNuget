using BaseNetCore.Core.src.Main.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BaseNetCore.Core.src.Main.Extensions.Permission
{
    /// <summary>
    /// Enterprise-grade Cached User Permission Service
    /// OPTIMIZED: Uses pre-configured JsonSerializerOptions and efficient caching
    /// </summary>
    public class CachedUserPermissionService : IUserPermissionService
    {
        private readonly ICorePermissionService _permissionService;
        private readonly ICacheService _cache;
        private readonly ILogger<CachedUserPermissionService> _logger;

        // OPTIMIZATION: Pre-configure cache options and JSON serializer options
        private static readonly DistributedCacheEntryOptions _cacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        };

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultBufferSize = 512, // Reduce allocation for small arrays
            WriteIndented = false // Compact JSON
        };

        public CachedUserPermissionService(
     ICorePermissionService permissionRepository,
   ICacheService cache,
ILogger<CachedUserPermissionService> logger)
        {
            _permissionService = permissionRepository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<IReadOnlyList<string>> GetPermissionsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return Array.Empty<string>();
            }

            string cacheKey = $"perms:{userId}";

            try
            {
                // OPTIMIZATION: Try get from cache first
                byte[]? cachedData = await _cache.GetAsync<byte[]>(cacheKey);
                if (cachedData != null && cachedData.Length > 0)
                {
                    // OPTIMIZATION: Deserialize directly from UTF8 bytes
                    var cachedPerms = JsonSerializer.Deserialize<List<string>>(cachedData, _jsonOptions);
                    if (cachedPerms != null)
                    {
                        return cachedPerms;
                    }
                }
            }
            catch (JsonException jsonEx)
            {
                _logger.LogWarning(jsonEx, "Failed to deserialize cached permissions for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get user permissions from cache for user {UserId}", userId);
            }

            // Cache miss or deserialization failed - fetch from repository
            var permsFromDb = await _permissionService.GetPermissionsByUserIdAsync(userId);
            var permsList = permsFromDb.ToList();

            try
            {
                // OPTIMIZATION: Serialize to UTF8 bytes directly
                var data = JsonSerializer.SerializeToUtf8Bytes(permsList, _jsonOptions);
                await _cache.SetAsync(cacheKey, data, _cacheOptions.AbsoluteExpirationRelativeToNow);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to set user permissions to cache for user {UserId}", userId);
            }

            return permsList;
        }

        public async Task<bool> UserHasPermissionAsync(string userId, string permission)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(permission))
            {
                return false;
            }

            var perms = await GetPermissionsAsync(userId);
            return perms.Contains(permission, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Invalidate cache for specific user
        /// </summary>
        public async Task InvalidateCacheAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return;

            try
            {
                string cacheKey = $"perms:{userId}";
                await _cache.RemoveAsync(cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to invalidate cache for user {UserId}", userId);
            }
        }
    }
}
