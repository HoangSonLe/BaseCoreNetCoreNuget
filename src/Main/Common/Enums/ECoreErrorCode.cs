using System.ComponentModel;

namespace BaseNetCore.Core.src.Main.Common.Enums
{
    /// <summary>
    /// Represents core error codes used throughout the application.
    /// Each value is associated with a description for error handling and display.
    /// </summary>
    public enum ECoreErrorCode
    {
        [Description("SYS001|Có lỗi phát sinh. Vui lòng liên hệ để xử lý.")]
        SYSTEM_ERROR,

        [Description("SYS002|Xung đột dữ liệu.")]
        CONFLICT,

        [Description("SYS003|Không có quyền truy cập dữ liệu.")]
        FORBIDDEN,

        [Description("SYS004|Dữ liệu yêu cầu không hợp lệ.")]
        REQUEST_INVALID,

        [Description("SYS005|Dữ liệu không tồn tại.")]
        RESOURCE_NOT_FOUND,

        [Description("SYS006|Có lỗi phát sinh. Vui lòng liên hệ để xử lý.")]
        SERVER_ERROR,

        [Description("SYS007|Phiên làm việc đã hết hạn.\nVui lòng thực hiện đăng nhập lại để sử dụng ứng dụng.")]
        TOKEN_INVALID,

        [Description("SYS008|Không có quyền truy cập dữ liệu.")]
        SYSTEM_AUTHORIZATION,

        [Description("SYS009|Dịch vụ không có sẵn")]
        SERVICE_UNAVAILABLE,

        [Description("SYS010|Dữ liệu yêu cầu không hợp lệ.")]
        BAD_REQUEST,

        [Description("SYS011|Too Many Requests")]
        TOO_MANY_REQUESTS
    }
}
