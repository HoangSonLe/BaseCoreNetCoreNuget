using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BaseNetCore.Core.src.Main.Common.Models
{
    [SwaggerSchema(Description = "Phản hồi dữ liệu có phân trang")]
    public class PageResponse<T>
    {
        [Required]
        [SwaggerSchema(Description = "Danh sách dữ liệu trong trang hiện tại", Required = new[] { "true" })]
        public List<T> Data { get; init; } = new();

        [SwaggerSchema(Description = "Trạng thái phản hồi", Required = new[] { "Success" })]
        public bool Success { get; init; } = true;

        [Required]
        [SwaggerSchema(Description = "Tổng số phần tử", Required = new[] { "true" })]
        public long Total { get; init; }

        [Required]
        [SwaggerSchema(Description = "Số trang hiện tại (bắt đầu từ 1)", Required = new[] { "true" })]
        public int CurrentPage { get; init; }

        [Required]
        [SwaggerSchema(Description = "Kích thước của mỗi trang", Required = new[] { "true" })]
        public int PageSize { get; init; }

        [JsonConstructor]
        public PageResponse(List<T> data, bool success, long total, int currentPage, int pageSize)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Total = total >= 0 ? total : throw new ArgumentException("Total must not be negative");
            CurrentPage = currentPage >= 1 ? currentPage : throw new ArgumentException("CurrentPage must be >= 1");
            PageSize = pageSize >= 1 ? pageSize : throw new ArgumentException("PageSize must be >= 1");
            Success = success;
        }

        public PageResponse() { } // hỗ trợ serializer / mapping
    }
}
