using Swashbuckle.AspNetCore.Annotations;

namespace BaseNetCore.Core.src.Main.Common.Models
{
    [SwaggerSchema(Description = "Phản hồi dữ liệu đơn giá trị")]
    public record ValueResponse<T>(
      [property: SwaggerSchema(Description = "Giá trị dữ liệu")]
      T Value
    );
}