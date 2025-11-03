namespace BaseNetCore.Core.src.Main.Extensions.Permission
{
    public interface ICorePermissionService
    {
        Task<IReadOnlyList<string>> GetPermissionsByUserIdAsync(string userId);
        Task<bool> UserHasPermissionAsync(string userId, string permission);
    }
}
