namespace BaseNetCore.Core.src.Main.Extensions.Permission
{
    public interface IUserPermissionService
    {
        Task<IReadOnlyList<string>> GetPermissionsAsync(string userId);
        Task<bool> UserHasPermissionAsync(string userId, string permission);
    }
}