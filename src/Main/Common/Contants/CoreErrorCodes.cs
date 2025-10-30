using BaseNetCore.Core.src.Main.Common.Enums;
using BaseNetCore.Core.src.Main.Common.Interfaces;
using System.Text.Json.Serialization;

namespace BaseNetCore.Core.src.Main.Common.Contants
{
    /// <summary>
    /// Represents a set of core error codes and their associated messages used throughout the application.
    /// Implements IErrorCode to allow extension by applications.
    /// </summary>
    public sealed class CoreErrorCodes : IErrorCode
    {
        [JsonIgnore]
        public ECoreErrorCode Key { get; }

        [JsonPropertyName("code")]
        public string Code { get; }

        [JsonPropertyName("message")]
        public string Message { get; }

        private CoreErrorCodes(ECoreErrorCode key, string code, string message)
        {
            Key = key;
            Code = code;
            Message = message;
        }

        public static readonly CoreErrorCodes SYSTEM_ERROR =
            new CoreErrorCodes(ECoreErrorCode.SYSTEM_ERROR, "SYS001", "Có lỗi phát sinh. Vui lòng liên hệ để xử lý.");
        public static readonly CoreErrorCodes CONFLICT =
            new CoreErrorCodes(ECoreErrorCode.CONFLICT, "SYS002", "Xung đột dữ liệu.");
        public static readonly CoreErrorCodes FORBIDDEN =
            new CoreErrorCodes(ECoreErrorCode.FORBIDDEN, "SYS003", "Không có quyền truy cập dữ liệu.");
        public static readonly CoreErrorCodes REQUEST_INVALID =
            new CoreErrorCodes(ECoreErrorCode.REQUEST_INVALID, "SYS004", "Dữ liệu yêu cầu không hợp lệ.");
        public static readonly CoreErrorCodes RESOURCE_NOT_FOUND =
            new CoreErrorCodes(ECoreErrorCode.RESOURCE_NOT_FOUND, "SYS005", "Dữ liệu không tồn tại.");
        public static readonly CoreErrorCodes SERVER_ERROR =
            new CoreErrorCodes(ECoreErrorCode.SERVER_ERROR, "SYS006", "Có lỗi phát sinh. Vui lòng liên hệ để xử lý.");
        public static readonly CoreErrorCodes TOKEN_INVALID =
            new CoreErrorCodes(ECoreErrorCode.TOKEN_INVALID, "SYS007", "Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại.");
        public static readonly CoreErrorCodes TOKEN_EXPIRED =
            new CoreErrorCodes(ECoreErrorCode.TOKEN_EXPIRED, "SYS012", "Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại.");
        public static readonly CoreErrorCodes SYSTEM_AUTHORIZATION =
            new CoreErrorCodes(ECoreErrorCode.SYSTEM_AUTHORIZATION, "SYS008", "Không có quyền truy cập dữ liệu.");
        public static readonly CoreErrorCodes SERVICE_UNAVAILABLE =
            new CoreErrorCodes(ECoreErrorCode.SERVICE_UNAVAILABLE, "SYS009", "Dịch vụ không có sẵn.");
        public static readonly CoreErrorCodes BAD_REQUEST =
            new CoreErrorCodes(ECoreErrorCode.BAD_REQUEST, "SYS010", "Dữ liệu yêu cầu không hợp lệ.");
        public static readonly CoreErrorCodes TOO_MANY_REQUESTS =
            new CoreErrorCodes(ECoreErrorCode.TOO_MANY_REQUESTS, "SYS011", "Too Many Requests");


        private static readonly Dictionary<string, CoreErrorCodes> _codeMap;
        private static readonly Dictionary<ECoreErrorCode, CoreErrorCodes> _keyMap;

        static CoreErrorCodes()
        {
            _codeMap = new Dictionary<string, CoreErrorCodes>(StringComparer.OrdinalIgnoreCase)
            {
                { SYSTEM_ERROR.Code, SYSTEM_ERROR },
                { CONFLICT.Code, CONFLICT },
                { FORBIDDEN.Code, FORBIDDEN },
                { REQUEST_INVALID.Code, REQUEST_INVALID },
                { RESOURCE_NOT_FOUND.Code, RESOURCE_NOT_FOUND },
                { SERVER_ERROR.Code, SERVER_ERROR },
                { TOKEN_INVALID.Code, TOKEN_INVALID },
                { SYSTEM_AUTHORIZATION.Code, SYSTEM_AUTHORIZATION },
                { SERVICE_UNAVAILABLE.Code, SERVICE_UNAVAILABLE },
                { BAD_REQUEST.Code, BAD_REQUEST },
                { TOO_MANY_REQUESTS.Code, TOO_MANY_REQUESTS },
                { TOKEN_EXPIRED.Code, TOKEN_EXPIRED }
            };

            _keyMap = new Dictionary<ECoreErrorCode, CoreErrorCodes>
            {
                { SYSTEM_ERROR.Key, SYSTEM_ERROR },
                { CONFLICT.Key, CONFLICT },
                { FORBIDDEN.Key, FORBIDDEN },
                { REQUEST_INVALID.Key, REQUEST_INVALID },
                { RESOURCE_NOT_FOUND.Key, RESOURCE_NOT_FOUND },
                { SERVER_ERROR.Key, SERVER_ERROR },
                { TOKEN_INVALID.Key, TOKEN_INVALID },
                { SYSTEM_AUTHORIZATION.Key, SYSTEM_AUTHORIZATION },
                { SERVICE_UNAVAILABLE.Key, SERVICE_UNAVAILABLE },
                { BAD_REQUEST.Key, BAD_REQUEST },
                { TOO_MANY_REQUESTS.Key, TOO_MANY_REQUESTS },
                { TOKEN_EXPIRED.Key, TOKEN_EXPIRED   }
            };

            var missing = Enum.GetValues(typeof(ECoreErrorCode))
                        .Cast<ECoreErrorCode>()
                        .Except(_keyMap.Keys)
                        .ToList();

            if (missing.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Missing CoreErrorCodes definition for: {string.Join(", ", missing)}");
            }
        }



        /// <summary>
        /// Retrieves the <see cref="CoreErrorCodes"/> instance associated with the specified <see cref="ECoreErrorCode"/> key.
        /// </summary>
        /// <param name="key">The <see cref="ECoreErrorCode"/> key to look up.</param>
        /// <returns>The corresponding <see cref="CoreErrorCodes"/> instance if found; otherwise, <c>null</c>.</returns>
        public static CoreErrorCodes? FromKey(ECoreErrorCode key)
        {
            if (_keyMap.TryGetValue(key, out var result))
                return result;
            return null;
        }

        /// <summary>
        /// Retrieves the <see cref="CoreErrorCodes"/> instance associated with the specified error code string.
        /// </summary>
        /// <param name="code">The error code string to look up.</param>
        /// <returns>The corresponding <see cref="CoreErrorCodes"/> instance if found; otherwise, <c>null</c>.</returns>
        public static CoreErrorCodes? FromCode(string code)
        {
            if (_codeMap.TryGetValue(code, out var result))
                return result;
            return null;
        }
    }
}
