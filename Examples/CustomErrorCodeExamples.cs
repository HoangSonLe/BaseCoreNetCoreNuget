using BaseNetCore.Core.src.Main.Common.Exceptions;
using BaseNetCore.Core.src.Main.Common.Interfaces;
using System.Net;

namespace BaseNetCore.Core.Examples
{
    /// <summary>
    /// Examples of how to define custom error codes in your application.
    /// </summary>
    internal class CustomErrorCodeExamples
    {
        /// <summary>
        /// Example 1: Define custom error codes using a sealed class (Recommended)
        /// </summary>
        internal class ProductErrorCodes : IErrorCode
        {
            public string Code { get; }
            public string Message { get; }

            private ProductErrorCodes(string code, string message)
            {
                Code = code;
                Message = message;
            }

            // Define your custom error codes
            public static readonly ProductErrorCodes PRODUCT_NOT_FOUND =
                new ProductErrorCodes("PRD001", "Sản phẩm không tồn tại");

            public static readonly ProductErrorCodes PRODUCT_OUT_OF_STOCK =
                        new ProductErrorCodes("PRD002", "Sản phẩm hết hàng");

            public static readonly ProductErrorCodes PRODUCT_PRICE_INVALID =
              new ProductErrorCodes("PRD003", "Giá sản phẩm không hợp lệ");

            public static readonly ProductErrorCodes PRODUCT_DUPLICATE =
                 new ProductErrorCodes("PRD004", "Sản phẩm đã tồn tại");
        }

        /// <summary>
        /// Example 2: Define custom exceptions using your error codes
        /// </summary>
        internal class ProductNotFoundException : BaseApplicationException
        {
            public ProductNotFoundException()
                 : base(ProductErrorCodes.PRODUCT_NOT_FOUND, ProductErrorCodes.PRODUCT_NOT_FOUND.Message, HttpStatusCode.NotFound)
            {
            }

            public ProductNotFoundException(string message)
                    : base(ProductErrorCodes.PRODUCT_NOT_FOUND, message, HttpStatusCode.NotFound)
            {
            }

            public ProductNotFoundException(int productId)
                       : base(ProductErrorCodes.PRODUCT_NOT_FOUND, $"Sản phẩm {productId} không tồn tại", HttpStatusCode.NotFound)
            {
            }
        }

        internal class ProductOutOfStockException : BaseApplicationException
        {
            public ProductOutOfStockException()
                : base(ProductErrorCodes.PRODUCT_OUT_OF_STOCK, ProductErrorCodes.PRODUCT_OUT_OF_STOCK.Message, HttpStatusCode.Conflict)
            {
            }

            public ProductOutOfStockException(string productName)
       : base(ProductErrorCodes.PRODUCT_OUT_OF_STOCK, $"Sản phẩm '{productName}' đã hết hàng", HttpStatusCode.Conflict)
            {
     }
        }

     /// <summary>
 /// Example 3: Using custom error codes in services
 /// </summary>
        internal class ProductService
   {
    public void ValidateProduct(int productId, decimal price, int stock)
            {
     // Use custom exceptions
     if (productId <= 0)
   throw new ProductNotFoundException(productId);

         if (price <= 0)
               throw new BaseApplicationException(
            ProductErrorCodes.PRODUCT_PRICE_INVALID,
     $"Giá sản phẩm {price} không hợp lệ. Giá phải lớn hơn 0.",
          HttpStatusCode.BadRequest);

          if (stock <= 0)
        throw new ProductOutOfStockException("iPhone 15 Pro");
     }
      }

        /// <summary>
        /// Example 4: Define error codes for different modules
        /// </summary>
   internal class OrderErrorCodes : IErrorCode
        {
            public string Code { get; }
   public string Message { get; }

   private OrderErrorCodes(string code, string message)
            {
                Code = code;
         Message = message;
   }

     public static readonly OrderErrorCodes ORDER_NOT_FOUND =
        new OrderErrorCodes("ORD001", "Đơn hàng không tồn tại");

      public static readonly OrderErrorCodes ORDER_ALREADY_CANCELLED =
    new OrderErrorCodes("ORD002", "Đơn hàng đã bị hủy");

       public static readonly OrderErrorCodes ORDER_CANNOT_CANCEL =
        new OrderErrorCodes("ORD003", "Không thể hủy đơn hàng đang giao");
        }

        /// <summary>
        /// Example 5: User management error codes
 /// </summary>
        internal class UserErrorCodes : IErrorCode
      {
    public string Code { get; }
         public string Message { get; }

   private UserErrorCodes(string code, string message)
            {
     Code = code;
     Message = message;
  }

            public static readonly UserErrorCodes USER_NOT_FOUND =
      new UserErrorCodes("USR001", "Người dùng không tồn tại");

        public static readonly UserErrorCodes USER_EMAIL_EXISTS =
   new UserErrorCodes("USR002", "Email đã được sử dụng");

        public static readonly UserErrorCodes USER_INACTIVE =
    new UserErrorCodes("USR003", "Tài khoản đã bị vô hiệu hóa");

   public static readonly UserErrorCodes USER_PASSWORD_INVALID =
          new UserErrorCodes("USR004", "Mật khẩu không đúng");
      }

        /// <summary>
        /// Example 6: Using with Controllers
        /// </summary>
   [Microsoft.AspNetCore.Mvc.ApiController]
      [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
        internal class ProductsController : Microsoft.AspNetCore.Mvc.ControllerBase
        {
         [Microsoft.AspNetCore.Mvc.HttpGet("{id}")]
            public Microsoft.AspNetCore.Mvc.IActionResult GetProduct(int id)
 {
        if (id <= 0)
       throw new ProductNotFoundException(id);

                // ... business logic

     return Ok(new { Id = id, Name = "Product" });
  }

    [Microsoft.AspNetCore.Mvc.HttpPost]
        public Microsoft.AspNetCore.Mvc.IActionResult CreateProduct([Microsoft.AspNetCore.Mvc.FromBody] CreateProductDto dto)
    {
         // Check duplicate
             bool exists = false; // ... check logic
  if (exists)
           throw new BaseApplicationException(
        ProductErrorCodes.PRODUCT_DUPLICATE,
       $"Sản phẩm '{dto.Name}' đã tồn tại",
         HttpStatusCode.Conflict);

      // Validate price
if (dto.Price <= 0)
     throw new BaseApplicationException(
   ProductErrorCodes.PRODUCT_PRICE_INVALID,
    "Giá sản phẩm phải lớn hơn 0",
        HttpStatusCode.BadRequest);

     return CreatedAtAction(nameof(GetProduct), new { id = 1 }, dto);
   }
        }

        internal class CreateProductDto
        {
            public string Name { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public int Stock { get; set; }
        }
    }

    /// <summary>
  /// Example API responses with custom error codes
    /// </summary>
    internal class CustomErrorResponseExamples
    {
        /// <summary>
   /// Response when product not found (HTTP 404):
        /// {
        ///   "guid": "abc-123",
  ///   "code": "PRD001",
        ///   "message": "Sản phẩm 999 không tồn tại",
 ///   "path": "/api/products/999",
        ///   "method": "GET",
        ///   "timestamp": "2025-01-28T10:30:00Z"
        /// }
     /// </summary>
        public void ProductNotFoundResponse() { }

        /// <summary>
        /// Response when product out of stock (HTTP 409):
   /// {
        ///   "guid": "abc-123",
   ///   "code": "PRD002",
   ///   "message": "Sản phẩm 'iPhone 15 Pro' đã hết hàng",
        ///   "path": "/api/orders",
     ///   "method": "POST",
        ///   "timestamp": "2025-01-28T10:30:00Z"
  /// }
  /// </summary>
      public void ProductOutOfStockResponse() { }

   /// <summary>
        /// Response when user email exists (HTTP 409):
    /// {
    ///   "guid": "abc-123",
      ///   "code": "USR002",
        ///   "message": "Email đã được sử dụng",
        /// "path": "/api/users",
  ///   "method": "POST",
        ///   "timestamp": "2025-01-28T10:30:00Z"
     /// }
   /// </summary>
        public void UserEmailExistsResponse() { }
    }
}
