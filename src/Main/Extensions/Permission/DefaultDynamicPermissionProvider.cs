using BaseNetCore.Core.src.Main.Extensions.Permission.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace BaseNetCore.Core.src.Main.Extensions.Permission
{
    /// <summary>
    /// Enterprise-grade Dynamic Permission Provider with Regex caching
    /// Optimized for performance and memory efficiency
    /// </summary>
    internal class DefaultDynamicPermissionProvider : IDynamicPermissionProvider
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DefaultDynamicPermissionProvider> _logger;
        private readonly IReadOnlyList<DynamicPermissionRule> _rules;
        private readonly IReadOnlyList<string> _permitAll;

        // OPTIMIZATION: Cache compiled regexes to avoid recompilation
        private static readonly ConcurrentDictionary<string, Regex> _regexCache = new();
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1); // Prevent ReDoS attacks

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
            _logger.LogInformation("Loaded {Count} dynamic permission rules", _rules.Count);
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
            var method = parts.Length >= 2 && !string.IsNullOrWhiteSpace(parts[1])
                ? parts[1].Trim().ToUpperInvariant()
                : "GET";

            var permsPart = parts.Length == 3 ? parts[2].Trim() : string.Empty;
            if (permsPart.StartsWith("@")) permsPart = permsPart.Substring(1);
            permsPart = permsPart.Trim().TrimEnd(',');

            var perms = permsPart.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(p => NormalizePermissionToken(p))
                                .ToArray();

            // OPTIMIZATION: Use cached regex compilation
            var regexPattern = BuildRegexPattern(pathPattern);
            var regex = GetOrCreateRegex(regexPattern);

            return new DynamicPermissionRule
            {
                RawPathPattern = raw,
                PathRegex = regex,
                HttpMethod = method,
                RequiredPermissions = perms
            };
        }

        /// <summary>
        /// Build regex pattern from path pattern with placeholders
        /// </summary>
        private static string BuildRegexPattern(string pathPattern)
        {
            // Ensure replacement of the {REGEX} token happens before escaping.
            const string token = "{REGEX}";
            const string placeholder = "__DYNAMIC_REGEX_PLACEHOLDER__";

            var preprocessed = pathPattern.Replace(token, placeholder);
            var escaped = Regex.Escape(preprocessed);

            // Replace the escaped placeholder with the runtime regex group
            escaped = escaped.Replace(Regex.Escape(placeholder), "(.+?)");

            // Convert wildcard markers to regex (escaped '*' becomes '\*')
            escaped = escaped.Replace(@"\*", "[^/]+");

            return "^" + escaped + "$";
        }

        /// <summary>
        /// OPTIMIZATION: Get or create compiled regex with caching
        /// This prevents repeated regex compilation for the same pattern
        /// </summary>
        private static Regex GetOrCreateRegex(string pattern)
        {
            return _regexCache.GetOrAdd(pattern, p =>
            {
                try
                {
                    // SECURITY: Add timeout to prevent ReDoS attacks
                    return new Regex(p, RegexOptions.Compiled | RegexOptions.IgnoreCase, RegexTimeout);
                }
                catch (ArgumentException ex)
                {
                    // Fallback to non-compiled regex if compilation fails
                    return new Regex(p, RegexOptions.IgnoreCase, RegexTimeout);
                }
            });
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
