# ?? Middleware Pipeline

## Overview

BaseNetCore.Core middleware pipeline:

```
Request
  │
CorrelationIdMiddleware (X-Correlation-ID)
  │
SerilogRequestLogging (Optional - if Serilog configured)
  │
GlobalExceptionMiddleware
  │
UseRouting
  │
UseAuthentication (JWT)
  │
UseAuthorization
  │
DynamicPermissionMiddleware (Optional)
  │
Controllers
  │
Response
```

---

## Setup

### Option 1: Full Pipeline với Auth

```csharp
var app = builder.Build();

app.UseBaseNetCoreMiddlewareWithAuth();
// Includes:
// - CorrelationIdMiddleware (X-Correlation-ID tracking)
// - SerilogRequestLogging (if Serilog configured)
// - GlobalExceptionMiddleware
// - UseAuthentication
// - UseAuthorization

app.MapControllers();
```

### Option 2: Without Auth

```csharp
var app = builder.Build();

app.UseBaseNetCoreMiddleware();
// Includes:
// - CorrelationIdMiddleware (X-Correlation-ID tracking)
// - SerilogRequestLogging (if Serilog configured)
// - GlobalExceptionMiddleware only

app.MapControllers();
```

### Option 3: Manual Setup

```csharp
var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>(); // Optional but recommended
app.UseSerilogRequestLogging(); // Optional - if using Serilog
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<DynamicPermissionMiddleware>();

app.MapControllers();
```

---

## Middleware Order

⚠️ **Order matters!**

```csharp
// ✅ CORRECT
app.UseMiddleware<CorrelationIdMiddleware>();  // 1. First - for tracking
app.UseSerilogRequestLogging();  // 2. Request logging (optional)
app.UseMiddleware<GlobalExceptionMiddleware>();  // 3. Exception handling
app.UseRouting();  // 4. Routing
app.UseAuthentication();// 5. Authentication
app.UseAuthorization();  // 6. Authorization
app.UseMiddleware<DynamicPermissionMiddleware>();  // 7. After auth
app.MapControllers();

// ❌ WRONG
app.UseAuthentication();  // ❌ Before routing
app.UseRouting();
app.MapControllers();
```

---

## Custom Middleware

```csharp
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
  private readonly ILogger<RequestLoggingMiddleware> _logger;

 public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
  {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
    _logger.LogInformation("Request: {Method} {Path}", 
            context.Request.Method, 
            context.Request.Path);

        await _next(context);

        _logger.LogInformation("Response: {StatusCode}", 
     context.Response.StatusCode);
    }
}

// Register
app.UseMiddleware<RequestLoggingMiddleware>();
```

---

**[? Back to Documentation](../README.md)**
