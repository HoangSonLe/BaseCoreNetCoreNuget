namespace BaseNetCore.Core.src.Main.Security.Permission
{
    public interface IDynamicPermissionProvider
    {
        Task<IReadOnlyList<DynamicPermissionRule>> GetRulesAsync();
        Task<IReadOnlyList<string>> GetPermitAllAsync();
    }
}
