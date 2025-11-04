# ?? Serilog Extensions for BaseNetCore.Core

This folder contains Serilog integration extensions for BaseNetCore.Core framework.

## ?? Overview

**Serilog** is a powerful structured logging library for .NET. BaseNetCore.Core provides seamless integration with zero-configuration support.

### ? Key Features

- ? **OPTIONAL** - Completely optional, not required
- ? **Zero Configuration** - Works with sensible defaults
- ? **Configuration Support** - Read from appsettings.json
- ? **Request Logging** - Automatic HTTP request logging
- ? **Structured Logging** - Rich context and properties
- ? **Multiple Sinks** - Console, File, Database, Cloud
- ? **Enrichers** - Machine name, Thread ID, Environment, etc.

## ?? Quick Start

### Minimal Setup (Zero Configuration)

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add Serilog with defaults
builder.AddBaseNetCoreSerilog();

// ... other configurations

var app = builder.Build();

// Middleware includes Serilog request logging automatically
app.UseBaseNetCoreMiddlewareWithAuth();

try
{
    app.Run();
}
finally
{
    app.FlushBaseNetCoreSerilog();
}
```

### With appsettings.json Configuration

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/log-.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

## ?? Files

### `SerilogExtensions.cs`

Main extension methods for Serilog integration:

#### `AddBaseNetCoreSerilog()`
Configures Serilog for BaseNetCore applications.

**Features:**
- Auto-reads from appsettings.json "Serilog" section
- Falls back to sensible defaults if no config
- Adds enrichers: FromLogContext, MachineName, ThreadId, Application, Environment
- Supports custom configuration via Action<LoggerConfiguration>

**Usage:**
```csharp
// Default
builder.AddBaseNetCoreSerilog();

// With custom config
builder.AddBaseNetCoreSerilog(config =>
{
    config.MinimumLevel.Debug();
    config.WriteTo.Seq("http://localhost:5341");
});
```

#### `UseBaseNetCoreSerilogRequestLogging()`
Adds Serilog request logging middleware with recommended settings.

**Features:**
- Logs HTTP requests with method, path, status code, elapsed time
- Custom log levels based on response (errors = Error, slow = Warning)
- Enriches with: Host, Scheme, UserAgent, RemoteIP, UserId, Username
- Customizable message template and enrichers

**Usage:**
```csharp
// After UseRouting(), before UseAuthentication()
app.UseBaseNetCoreSerilogRequestLogging();

// With custom options
app.UseBaseNetCoreSerilogRequestLogging(options =>
{
    options.MessageTemplate = "Custom template: {RequestMethod} {RequestPath}";
});
```

#### `FlushBaseNetCoreSerilog()`
Ensures logs are flushed on application shutdown.

**Usage:**
```csharp
try
{
    app.Run();
}
finally
{
    app.FlushBaseNetCoreSerilog();
}
```

## ?? Default Configuration

When no "Serilog" section exists in appsettings.json:

### Development
- **MinimumLevel:** Debug
- **Sinks:** Console only
- **Output:** Colored console with timestamp and source context

### Production
- **MinimumLevel:** Information
- **Sinks:** Console + File
- **File Path:** `logs/log-.txt` (rolling daily)
- **Retention:** 30 days
- **File Size Limit:** 10MB with rollover

### Overrides
- **Microsoft:** Warning
- **Microsoft.AspNetCore:** Warning
- **Microsoft.EntityFrameworkCore:** Warning
- **System:** Warning
- **BaseNetCore.Core:** Debug

## ?? Structured Logging Examples

### Basic Logging

```csharp
public class ProductService
{
    private readonly ILogger<ProductService> _logger;

    public async Task<Product> GetProduct(int id)
    {
        _logger.LogInformation("Getting product: {ProductId}", id);
        
        var product = await _repository.GetByIdAsync(id);
        
        if (product == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", id);
        }
        
        return product;
    }
}
```

### With Complex Objects

```csharp
// Use @ for destructuring
_logger.LogDebug("User details: {@User}", user);
_logger.LogInformation("Order created: {@Order}", order);
```

### With Scopes

```csharp
using (_logger.BeginScope("OrderId={OrderId}", orderId))
{
    _logger.LogInformation("Processing payment");
    _logger.LogInformation("Sending notification");
    // All logs include OrderId
}
```

## ?? Related Middleware

### CorrelationIdMiddleware

BaseNetCore.Core includes `CorrelationIdMiddleware` for distributed tracing:

```csharp
// Automatically added by UseBaseNetCoreMiddleware()
app.UseMiddleware<CorrelationIdMiddleware>();
```

**Features:**
- Generates or reads `X-Correlation-ID` header
- Adds to Serilog LogContext
- Includes in all logs during request
- Returns in response headers

**Log output:**
```
[14:30:45 INF] Processing request {CorrelationId="abc-123"}
[14:30:45 INF] Database query {CorrelationId="abc-123"}
```

## ?? Documentation

For complete documentation, see:

- **[Serilog Logging Guide](../../README/10-Extensions/Serilog-Logging.md)** - Full documentation
- **[Configuration Guide](../../README/01-Getting-Started/Configuration.md)** - Configuration examples
- **[Middleware Pipeline](../../README/09-Middleware/Middleware-Pipeline.md)** - Middleware order

## ?? External Resources

- [Serilog Official Documentation](https://serilog.net/)
- [Serilog ASP.NET Core](https://github.com/serilog/serilog-aspnetcore)
- [Serilog Best Practices](https://github.com/serilog/serilog/wiki/Best-Practices)

## ?? Tips

### Performance
- Use structured logging, not string interpolation
- Use LoggerMessage for high-performance scenarios
- Configure appropriate minimum levels per namespace

### Best Practices
- Always use structured properties: `{PropertyName}`
- Use `@` for complex objects: `{@Object}`
- Don't log sensitive data (passwords, tokens, credit cards)
- Use appropriate log levels (Debug < Information < Warning < Error)

### Production
- Write to persistent storage (File, Database, Cloud)
- Configure log retention policies
- Use Seq or similar for log analysis
- Monitor disk space for file logs

---

**[? Back to Main Documentation](../../README/README.md)**
