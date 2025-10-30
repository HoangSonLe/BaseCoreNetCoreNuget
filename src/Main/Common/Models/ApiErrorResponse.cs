using BaseNetCore.Core.src.Main.Utils;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Text.Json.Serialization;

namespace BaseNetCore.Core.src.Main.Common.Models
{
    public class ApiErrorResponse
    {
        [SwaggerSchema(Description = "Mã định danh request hoặc lỗi duy nhất", Nullable = false)]
        public string Guid { get; init; }

        [SwaggerSchema(Description = "Mã lỗi hệ thống hoặc business", Nullable = false)]
        public string Code { get; init; }

        [SwaggerSchema(Description = "Thông báo lỗi hoặc key lỗi", Nullable = false)]
        public string Message { get; init; }

        [SwaggerSchema(Description = "Đường dẫn API gây lỗi", Nullable = false)]
        public string Path { get; init; }

        [SwaggerSchema(Description = "Phương thức HTTP", Nullable = false)]
        public string Method { get; init; }

        [SwaggerSchema(Description = "Thời điểm xảy ra lỗi (UTC)", Nullable = false)]
        [JsonPropertyName("timestamp")]
        [JsonConverter(typeof(DateTimeJsonConverter))]
        public DateTime Timestamp { get; init; }

        [SwaggerSchema(Description = "Chi tiết lỗi validation (nếu có)", Nullable = true)]
        [JsonPropertyName("errors")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string[]>? Errors { get; init; }

        public static ApiErrorResponse Empty()
        {
            return new ApiErrorResponse
            {
                Guid = string.Empty,
                Code = string.Empty,
                Message = string.Empty,
                Path = string.Empty,
                Method = string.Empty,
                Timestamp = DateTime.MinValue
            };
        }

        public ApiErrorResponse()
        {
        }

        public ApiErrorResponse(string guid, string code, string message, Microsoft.AspNetCore.Http.HttpContext context)
        {
            Guid = guid;
            Code = code;
            Message = message;
            Path = context.Request.Path;
            Method = context.Request.Method;
            Timestamp = DateTime.UtcNow;
        }

        public ApiErrorResponse(string guid, string code, string message, Microsoft.AspNetCore.Http.HttpContext context, Dictionary<string, string[]>? errors)
        {
            Guid = guid;
            Code = code;
            Message = message;
            Path = context.Request.Path;
            Method = context.Request.Method;
            Timestamp = DateTime.UtcNow;
            Errors = errors;
        }
    }
}
