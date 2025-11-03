using BaseNetCore.Core.src.Main.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text;

namespace BaseNetCore.Core.src.Main.Security.Permission
{
    public class CachedUserPermissionService : IUserPermissionService
    {
        private readonly ICorePermissionService _permissionService;
        private readonly ICacheService _cache;
        private readonly ILogger<CachedUserPermissionService> _logger;
        private static readonly DistributedCacheEntryOptions _cacheOptions = new DistributedCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
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
                byte[]? cachedData = await _cache.GetAsync<byte[]>(cacheKey);
                if (cachedData != null)
                {
                    var cachedJson = Encoding.UTF8.GetString(cachedData);
                    var cachedPerms = System.Text.Json.JsonSerializer.Deserialize<List<string>>(cachedJson);
                    if (cachedPerms != null)
                    {
                        return cachedPerms;
                    }
                }
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
                var json = System.Text.Json.JsonSerializer.Serialize(permsList);
                var data = Encoding.UTF8.GetBytes(json);
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
    }
}
