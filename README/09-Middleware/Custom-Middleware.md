# ??? Custom Middleware

## Creating Custom Middleware

```csharp
public class CustomMiddleware
{
    private readonly RequestDelegate _next;

    public CustomMiddleware(RequestDelegate next)
    {
   _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Before logic
     
  await _next(context);
        
     // After logic
    }
}

// Extension method
public static class CustomMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CustomMiddleware>();
    }
}

// Usage
app.UseCustomMiddleware();
```

---

## Examples

### 1. Request Timing

```csharp
public class RequestTimingMiddleware
{
 private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
   var sw = Stopwatch.StartNew();
        
        await _next(context);
        
     sw.Stop();
        _logger.LogInformation("Request {Method} {Path} took {ElapsedMs}ms",
  context.Request.Method,
            context.Request.Path,
    sw.ElapsedMilliseconds);
    }
}
```

### 2. API Key Validation

```csharp
public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _apiKeyHeader = "X-API-Key";

public async Task InvokeAsync(HttpContext context)
    {
   if (!context.Request.Headers.TryGetValue(_apiKeyHeader, out var apiKey))
      {
     context.Response.StatusCode = 401;
await context.Response.WriteAsync("API Key required");
   return;
      }

  if (!IsValidApiKey(apiKey))
        {
  context.Response.StatusCode = 403;
      await context.Response.WriteAsync("Invalid API Key");
return;
        }

        await _next(context);
    }
}
```

### 3. Request/Response Logging

```csharp
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public async Task InvokeAsync(HttpContext context)
    {
 // Log request
        context.Request.EnableBuffering();
        var requestBody = await ReadRequestBodyAsync(context.Request);
     _logger.LogInformation("Request: {Method} {Path} {Body}",
    context.Request.Method,
   context.Request.Path,
 requestBody);

     // Capture response
   var originalBody = context.Response.Body;
   using var newBody = new MemoryStream();
        context.Response.Body = newBody;

  await _next(context);

        // Log response
     newBody.Seek(0, SeekOrigin.Begin);
   var responseBody = await new StreamReader(newBody).ReadToEndAsync();
      _logger.LogInformation("Response: {StatusCode} {Body}",
            context.Response.StatusCode,
  responseBody);

        newBody.Seek(0, SeekOrigin.Begin);
        await newBody.CopyToAsync(originalBody);
    }
}
```

### 4. Correlation ID Middleware (Built-in)

BaseNetCore.Core bao g?m CorrelationIdMiddleware ?? tracking requests:

```csharp
// Add to middleware pipeline
app.UseMiddleware<CorrelationIdMiddleware>();

// All logs will include CorrelationId
// Response headers will include X-Correlation-ID for client tracking
```

**Log output:**
```
[14:30:45 INF] Processing request {CorrelationId="abc-123-def-456"}
[14:30:45 INF] Database query executed {CorrelationId="abc-123-def-456"}
[14:30:45 INF] Request completed {CorrelationId="abc-123-def-456"}
```

---

**[? Back to Documentation](../README.md)**
