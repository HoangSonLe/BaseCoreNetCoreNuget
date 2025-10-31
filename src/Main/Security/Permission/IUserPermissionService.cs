using System.Collections.Generic;
using System.Threading.Tasks;

namespace BaseNetCore.Core.src.Main.Security.Permission
{
    public interface IUserPermissionService
    {
        Task<IReadOnlyList<string>> GetPermissionsAsync(string userId);
        Task<bool> UserHasPermissionAsync(string userId, string permission);
    }
}