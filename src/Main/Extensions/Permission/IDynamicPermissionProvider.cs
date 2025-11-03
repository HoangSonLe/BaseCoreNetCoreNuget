using BaseNetCore.Core.src.Main.Extensions.Permission.Models;

namespace BaseNetCore.Core.src.Main.Extensions.Permission
{
    public interface IDynamicPermissionProvider
    {
        Task<IReadOnlyList<DynamicPermissionRule>> GetRulesAsync();
        Task<IReadOnlyList<string>> GetPermitAllAsync();
    }
}
