using BaseNetCore.Core.src.Main.Security.Permission.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace BaseNetCore.Core.src.Main.Security.Permission
{
    internal class DefaultDynamicPermissionProvider : IDynamicPermissionProvider
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DefaultDynamicPermissionProvider> _logger;
        private readonly IReadOnlyList<DynamicPermissionRule> _rules;
        private readonly IReadOnlyList<string> _permitAll;
        public DefaultDynamicPermissionProvider(
            IConfiguration configuration,
            ILogger<DefaultDynamicPermissionProvider> logger)
        {
            _configuration = configuration;
            _logger = logger;

            var section = _configuration.GetSection("DynamicPermissions");
            _permitAll = section.GetSection("PermitAll").Get<IReadOnlyList<string>>() ?? Array.Empty<string>();

            var list = new List<DynamicPermissionRule>();
            var permissionsSection = section.GetSection("Permissions");

            foreach (var svc in permissionsSection.GetChildren())
            {
                foreach (var raw in svc.Get<string[]>() ?? Array.Empty<string>())
                {
                    try
                    {
                        var rule = ParseRawRule(raw);
                        if (rule != null) list.Add(rule);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse dynamic-permission rule: {Raw}", raw);
                    }
                }
            }
            _rules = list;
        }
        public Task<IReadOnlyList<string>> GetPermitAllAsync()
        {
            return Task.FromResult(_permitAll);
        }

        public Task<IReadOnlyList<DynamicPermissionRule>> GetRulesAsync()
        {
            return Task.FromResult(_rules);
        }

        private static DynamicPermissionRule? ParseRawRule(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            var parts = raw.Split(':', StringSplitOptions.RemoveEmptyEntries);
            var pathPattern = parts.Length > 0 ? parts[0].Trim() : null;
            var method = parts.Length >= 2 && !string.IsNullOrWhiteSpace(parts[1]) ? parts[1].Trim().ToUpperInvariant() : "GET";
            var permsPart = parts.Length == 3 ? parts[2].Trim() : string.Empty;
            if (permsPart.StartsWith("@")) permsPart = permsPart.Substring(1);
            permsPart = permsPart.Trim().TrimEnd(',');
            var perms = permsPart.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(p => NormalizePermissionToken(p))
                                .ToArray();

            // Ensure replacement of the {REGEX} token happens before escaping.
            // Escaping then searching for an escaped token can be brittle across build/package boundaries.
            const string token = "{REGEX}";
            const string placeholder = "__DYNAMIC_REGEX_PLACEHOLDER__";

            var preprocessed = pathPattern.Replace(token, placeholder);
            var escaped = Regex.Escape(preprocessed);

            // Replace the escaped placeholder with the runtime regex group
            escaped = escaped.Replace(Regex.Escape(placeholder), "(.+)");

            // Convert wildcard markers to regex (escaped '*' becomes '\*')
            escaped = escaped.Replace(@"\*", "[^/]+");

            var regexPattern = "^" + escaped + "$";
            var regex = new Regex(regexPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            return new DynamicPermissionRule
            {
                RawPathPattern = raw,
                PathRegex = regex,
                HttpMethod = method,
                RequiredPermissions = perms
            };

        }
        private static string NormalizePermissionToken(string token)
        {
            token = token.Trim();
            while (token.StartsWith("/")) token = token.Substring(1);
            if (token.StartsWith("@")) token = token.Substring(1);
            return token;
        }
    }
}
