using System.Text.RegularExpressions;

namespace BaseNetCore.Core.src.Main.Extensions.Permission.Models
{
    public sealed class DynamicPermissionRule
    {
        public Regex PathRegex { get; init; } = null!;
        public string HttpMethod { get; init; } = null!;
        public IReadOnlyList<string> RequiredPermissions { get; init; } = Array.Empty<string>();
        public string RawPathPattern { get; init; } = string.Empty;
    }
}
