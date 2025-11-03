namespace BaseNetCore.Core.src.Main.Cache
{
    public interface ICacheService
    {
        /// <summary>
        /// Lấy giá trị từ cache
        /// </summary>
        Task<T?> GetAsync<T>(string key) where T : class;

        /// <summary>
        /// Lưu giá trị vào cache
        /// </summary>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;

        /// <summary>
        /// Xóa một key khỏi cache
        /// </summary>
        Task RemoveAsync(string key);

        /// <summary>
        /// Xóa nhiều keys khỏi cache (theo pattern)
        /// </summary>
        Task RemoveByPatternAsync(string pattern);

        /// <summary>
        /// Kiểm tra key có tồn tại không
        /// </summary>
        Task<bool> ExistsAsync(string key);
    }
}
