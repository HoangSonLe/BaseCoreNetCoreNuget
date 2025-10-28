using System.Globalization;

namespace BaseNetCore.Core.src.Main.Utils
{
    public static class StringHelper
    {
        /// <summary>
        /// Kiểm tra chuỗi null hoặc rỗng.
        /// </summary>
        public static bool IsNullOrEmpty(string str)
            => string.IsNullOrEmpty(str);

        /// <summary>
        /// Kiểm tra chuỗi null hoặc chỉ chứa khoảng trắng.
        /// </summary>
        public static bool IsNullOrWhiteSpace(string str)
            => string.IsNullOrWhiteSpace(str);

        /// <summary>
        /// Chuỗi có ký tự hợp lệ (không null, không rỗng, không toàn khoảng trắng).
        /// </summary>
        public static bool HasText(string str)
            => !string.IsNullOrWhiteSpace(str);

        /// <summary>
        /// Cắt bỏ khoảng trắng ở đầu và cuối, trả về null nếu kết quả là rỗng.
        /// </summary>
        public static string TrimToNull(string str)
        {
            if (str == null) return null;
            var trimmed = str.Trim();
            return trimmed.Length == 0 ? null : trimmed;
        }

        /// <summary>
        /// Cắt bỏ khoảng trắng ở đầu và cuối, trả về chuỗi rỗng nếu null.
        /// </summary>
        public static string TrimToEmpty(string str)
            => str?.Trim() ?? string.Empty;

        /// <summary>
        /// So sánh chuỗi không phân biệt hoa thường.
        /// </summary>
        public static bool EqualsIgnoreCase(string a, string b)
            => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Viết hoa ký tự đầu tiên, giữ nguyên phần còn lại.
        /// </summary>
        public static string Capitalize(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            if (str.Length == 1)
                return str.ToUpper();

            return char.ToUpper(str[0], CultureInfo.InvariantCulture) + str.Substring(1);
        }

        /// <summary>
        /// Viết thường ký tự đầu tiên, giữ nguyên phần còn lại.
        /// </summary>
        public static string Uncapitalize(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            if (str.Length == 1)
                return str.ToLower();

            return char.ToLower(str[0], CultureInfo.InvariantCulture) + str.Substring(1);
        }

        /// <summary>
        /// Cắt chuỗi nếu dài hơn độ dài chỉ định.
        /// </summary>
        public static string Truncate(string str, int maxLength)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            return str.Length <= maxLength ? str : str.Substring(0, maxLength);
        }

        /// <summary>
        /// Kiểm tra chuỗi có chứa substring không (phân biệt hoa thường).
        /// </summary>
        public static bool Contains(string source, string toCheck)
            => source?.Contains(toCheck) ?? false;

        /// <summary>
        /// Kiểm tra chuỗi có chứa substring không (không phân biệt hoa thường).
        /// </summary>
        public static bool ContainsIgnoreCase(string source, string toCheck)
        {
            if (source == null || toCheck == null)
                return false;

            return source.IndexOf(toCheck, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
