using BaseNetCore.Core.src.Main.Common.Attributes;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace BaseNetCore.Core.src.Main.Utils
{
    /// <summary>
    /// Utility class for building searchable strings from entities.
    /// Converts Vietnamese diacritics to non-unicode characters and concatenates searchable fields.
    /// Similar to Java SearchFieldUtils.
    /// </summary>
    public static class SearchFieldUtils
    {
        /// <summary>
        /// Builds a searchable non-unicode string from all properties marked with [SearchableField] attribute.
        /// Converts Vietnamese diacritics to non-unicode and concatenates fields with spaces.
        /// </summary>
        /// <param name="entity">Entity to build search string from</param>
        /// <returns>Non-unicode searchable string</returns>
        public static string BuildString(object entity)
        {
            if (entity == null)
                return string.Empty;

            var type = entity.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                  .Where(p => p.GetCustomAttribute<SearchableFieldAttribute>() != null)
                       .OrderBy(p => p.GetCustomAttribute<SearchableFieldAttribute>().Order)
            .ThenBy(p => p.Name);

            var sb = new StringBuilder();

            foreach (var property in properties)
            {
                var value = property.GetValue(entity);
                if (value != null)
                {
                    var stringValue = value.ToString();
                    if (!string.IsNullOrWhiteSpace(stringValue))
                    {
                        if (sb.Length > 0)
                            sb.Append(' ');

                        sb.Append(RemoveVietnameseDiacritics(stringValue));
                    }
                }
            }

            return sb.ToString().Trim().ToLowerInvariant();
        }

        /// <summary>
        /// Builds a searchable string from specified properties.
        /// </summary>
        /// <param name="entity">Entity to build search string from</param>
        /// <param name="propertyNames">Names of properties to include</param>
        /// <returns>Non-unicode searchable string</returns>
        public static string BuildStringFromProperties(object entity, params string[] propertyNames)
        {
            if (entity == null || propertyNames == null || propertyNames.Length == 0)
                return string.Empty;

            var type = entity.GetType();
            var sb = new StringBuilder();

            foreach (var propertyName in propertyNames)
            {
                var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (property != null)
                {
                    var value = property.GetValue(entity);
                    if (value != null)
                    {
                        var stringValue = value.ToString();
                        if (!string.IsNullOrWhiteSpace(stringValue))
                        {
                            if (sb.Length > 0)
                                sb.Append(' ');

                            sb.Append(RemoveVietnameseDiacritics(stringValue));
                        }
                    }
                }
            }

            return sb.ToString().Trim().ToLowerInvariant();
        }

        /// <summary>
        /// Removes Vietnamese diacritics from a string and converts to non-unicode.
        /// Examples: "Nguyễn Văn A" -> "nguyen van a", "Đặng" -> "dang"
        /// </summary>
        /// <param name="text">Text with Vietnamese diacritics</param>
        /// <returns>Non-unicode text</returns>
        public static string RemoveVietnameseDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Normalize to FormD (decomposed form) to separate base characters from diacritics
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalizedString)
            {
                // Skip combining diacritical marks
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            // Handle special Vietnamese characters that don't decompose properly
            var result = sb.ToString().Normalize(NormalizationForm.FormC);

            // Replace Đ/đ with D/d
            result = result.Replace('Đ', 'D').Replace('đ', 'd');

            return result;
        }

        /// <summary>
        /// Converts a string to search-friendly format (lowercase, no diacritics).
        /// Useful for search queries.
        /// </summary>
        /// <param name="text">Text to normalize</param>
        /// <returns>Normalized search text</returns>
        public static string NormalizeSearchText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            return RemoveVietnameseDiacritics(text).ToLowerInvariant().Trim();
        }

        /// <summary>
        /// Checks if a non-unicode search string contains the search term.
        /// </summary>
        /// <param name="searchString">Non-unicode search string from entity</param>
        /// <param name="searchTerm">Search term (will be normalized)</param>
        /// <returns>True if search string contains the term</returns>
        public static bool Contains(string searchString, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchString) || string.IsNullOrWhiteSpace(searchTerm))
                return false;

            var normalizedTerm = NormalizeSearchText(searchTerm);
            return searchString.Contains(normalizedTerm);
        }

        /// <summary>
        /// Checks if any of the search terms exist in the search string.
        /// </summary>
        /// <param name="searchString">Non-unicode search string from entity</param>
        /// <param name="searchTerms">Search terms (will be normalized)</param>
        /// <returns>True if any term is found</returns>
        public static bool ContainsAny(string searchString, params string[] searchTerms)
        {
            if (string.IsNullOrWhiteSpace(searchString) || searchTerms == null || searchTerms.Length == 0)
                return false;

            return searchTerms.Any(term => Contains(searchString, term));
        }

        /// <summary>
        /// Checks if all search terms exist in the search string.
        /// </summary>
        /// <param name="searchString">Non-unicode search string from entity</param>
        /// <param name="searchTerms">Search terms (will be normalized)</param>
        /// <returns>True if all terms are found</returns>
        public static bool ContainsAll(string searchString, params string[] searchTerms)
        {
            if (string.IsNullOrWhiteSpace(searchString) || searchTerms == null || searchTerms.Length == 0)
                return false;

            return searchTerms.All(term => Contains(searchString, term));
        }
    }
}
