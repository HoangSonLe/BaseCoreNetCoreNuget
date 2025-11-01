# ?? Global Exception Middleware

## Gi?i thi?u

**GlobalExceptionMiddleware** catches t?t c? unhandled exceptions và returns standardized **ApiErrorResponse**.

---

## How It Works

```csharp
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
  {
   try
        {
   await _next(context);
        }
        catch (Exception ex)
        {
  await HandleExceptionAsync(context, ex, context.TraceIdentifier);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex, string requestId)
    {
    _logger.LogError(ex, "Unhandled exception - RequestId: {RequestId}", requestId);

        var response = context.Response;
   response.ContentType = "application/json";

        ApiErrorResponse apiResponse;
        int statusCode;

  if (ex is BaseApplicationException appEx)
        {
 statusCode = (int)appEx.HttpStatus;
  apiResponse = new ApiErrorResponse(requestId, appEx.ErrorCode.Code, appEx.Message, context);
        }
   else
        {
            statusCode = 500;
    apiResponse = new ApiErrorResponse(requestId, "UNKNOWN", ex.Message, context);
   }

   response.StatusCode = statusCode;
    var json = JsonSerializer.Serialize(apiResponse, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });
        await response.WriteAsync(json);
 }
}
```

---

## Registration

```csharp
// Program.cs
var app = builder.Build();

// ? Add as FIRST middleware (before UseRouting)
app.UseMiddleware<GlobalExceptionMiddleware>();

// Or use extension method
app.UseBaseNetCoreMiddleware();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

---

## ApiErrorResponse

```csharp
public class ApiErrorResponse
{
    public string Guid { get; set; }
    public string Code { get; set; }
 public string Message { get; set; }
    public string Path { get; set; }
 public string Method { get; set; }
    public DateTime Timestamp { get; set; }
public Dictionary<string, string[]>? Errors { get; set; }

    public ApiErrorResponse(string guid, string code, string message, HttpContext context)
{
        Guid = guid;
        Code = code;
     Message = message;
        Path = context.Request.Path;
   Method = context.Request.Method;
        Timestamp = DateTime.UtcNow;
    }
}
```

---

## Response Examples

### BaseApplicationException

**Request:**
```http
GET /api/products/999
```

**Response: 404**
```json
{
  "guid": "0HN7Q3QK3V8Q1:00000001",
  "code": "SYS005",
  "message": "Product 999 not found",
  "path": "/api/products/999",
  "method": "GET",
  "timestamp": "2025-01-28T10:30:00.123Z"
}
```

### Unknown Exception

**Request:**
```http
POST /api/orders
```

**Response: 500**
```json
{
  "guid": "0HN7Q3QK3V8Q1:00000002",
  "code": "UNKNOWN",
  "message": "Object reference not set to an instance of an object.",
  "path": "/api/orders",
  "method": "POST",
  "timestamp": "2025-01-28T10:30:00.123Z"
}
```

---

## Logging

```csharp
// Middleware logs all exceptions
_logger.LogError(ex, "Unhandled exception - RequestId: {RequestId}", requestId);
```

**Log output:**
```
[ERR] Unhandled exception - RequestId: 0HN7Q3QK3V8Q1:00000001
System.InvalidOperationException: Product 999 not found
   at ProductService.GetProductById(Int32 id)
   at ProductsController.GetProduct(Int32 id)
```

---

## Custom Error Handling

```csharp
public class CustomGlobalExceptionMiddleware
{
    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        // Custom logic based on exception type
        if (ex is DbUpdateException dbEx)
        {
   _logger.LogError(dbEx, "Database error");
   await WriteError(context, 500, "DB_ERROR", "Database error occurred");
  }
        else if (ex is ValidationException valEx)
{
 _logger.LogWarning(valEx, "Validation error");
     await WriteError(context, 400, "VALIDATION_ERROR", valEx.Message);
        }
  else
   {
 _logger.LogError(ex, "Unexpected error");
      await WriteError(context, 500, "UNKNOWN", "An unexpected error occurred");
   }
    }
}
```

---

## Exception Filters (Alternative)

```csharp
public class GlobalExceptionFilter : IExceptionFilter
{
 private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger;
    }

 public void OnException(ExceptionContext context)
 {
        _logger.LogError(context.Exception, "Exception in {Action}", 
            context.ActionDescriptor.DisplayName);

  if (context.Exception is BaseApplicationException appEx)
    {
     context.Result = new ObjectResult(new ApiErrorResponse(
          context.HttpContext.TraceIdentifier,
      appEx.ErrorCode.Code,
    appEx.Message,
          context.HttpContext))
        {
         StatusCode = (int)appEx.HttpStatus
       };
    }
        else
        {
   context.Result = new ObjectResult(new ApiErrorResponse(
           context.HttpContext.TraceIdentifier,
    "UNKNOWN",
      context.Exception.Message,
       context.HttpContext))
       {
           StatusCode = 500
            };
        }

        context.ExceptionHandled = true;
    }
}

// Register
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});
```

---

## Best Practices

### ? DO

```csharp
// ? Log all exceptions
_logger.LogError(ex, "Unhandled exception");

// ? Include request ID for tracing
apiResponse.Guid = context.TraceIdentifier;

// ? Sanitize error messages in production
if (_env.IsProduction())
{
    message = "An error occurred. Please contact support.";
}
```

### ? DON'T

```csharp
// ? Expose stack traces to clients
return new { error = ex.ToString() };

// ? Catch and ignore
try { }
catch { /* silent */ }

// ? Return generic 500 for all errors
return StatusCode(500, "Error");
```

---

**[? Back to Documentation](../README.md)**
