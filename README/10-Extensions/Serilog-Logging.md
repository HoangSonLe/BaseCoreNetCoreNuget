# ?? Serilog Logging Extension

## M?c l?c
- [Gi?i thi?u](#-gi?i-thi?u)
- [Cài ??t](#-cài-??t)
- [Quick Start](#-quick-start)
- [C?u hình](#-c?u-hình)
- [S? d?ng Serilog](#-s?-d?ng-serilog)
- [Request Logging](#-request-logging)
- [Structured Logging](#-structured-logging)
- [Best Practices](#-best-practices)
- [Examples](#-examples)

---

## ?? Gi?i thi?u

**Serilog** là m?t th? vi?n logging m?nh m? và linh ho?t cho .NET v?i h? tr? structured logging. BaseNetCore.Core cung c?p extension methods ?? tích h?p Serilog m?t cách d? dàng v?i các tính n?ng:

? **OPTIONAL** - Hoàn toàn tùy ch?n, không b?t bu?c ph?i dùng  
? **Zero Configuration** - Ho?t ??ng v?i c?u hình m?c ??nh  
? **Flexible Configuration** - H? tr? c?u hình t? appsettings.json  
? **Request Logging** - T? ??ng log HTTP requests  
? **Structured Logging** - Log v?i context và properties  
? **Multiple Sinks** - Console, File, Database, Cloud services  

---

## ?? Cài ??t

### Packages ?ã ???c include trong BaseNetCore.Core

BaseNetCore.Core ?ã bao g?m các Serilog packages c?n thi?t:

```xml
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="Serilog.Enrichers.Environment" Version="3.0.1" />
<PackageReference Include="Serilog.Enrichers.Thread" Version="4.0.0" />
<PackageReference Include="Serilog.Extensions.Hosting" Version="8.0.0" />
<PackageReference Include="Serilog.Extensions.Logging" Version="8.0.0" />
<PackageReference Include="Serilog.Settings.Configuration" Version="8.0.0" />
```

### Optional Packages (n?u c?n thêm sinks)

```bash
# File sink (?ã có s?n trong Serilog.AspNetCore)
dotnet add package Serilog.Sinks.File

# Database sinks
dotnet add package Serilog.Sinks.MSSqlServer
dotnet add package Serilog.Sinks.PostgreSQL

# Cloud sinks
dotnet add package Serilog.Sinks.AzureAnalytics
dotnet add package Serilog.Sinks.Elasticsearch

# Seq (local dev tool)
dotnet add package Serilog.Sinks.Seq
```

---

## ?? Quick Start

### Option 1: Zero Configuration (Khuy?n ngh? cho b?t ??u)

```csharp
// Program.cs
using BaseNetCore.Core.src.Main.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Thêm Serilog v?i c?u hình m?c ??nh
builder.AddBaseNetCoreSerilog();

// Add DbContext và các services khác...
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddBaseNetCoreFeaturesWithAuth(builder.Configuration);

var app = builder.Build();

// Thêm middleware
app.UseRouting();
app.UseBaseNetCoreMiddlewareWithAuth(); // ?ã bao g?m Serilog request logging

app.MapControllers();

try
{
    app.Run();
}
finally
{
    // ??m b?o logs ???c flush khi shutdown
    app.FlushBaseNetCoreSerilog();
}
```

**Xong!** B?n ?ã có:
- ? Console logging v?i màu s?c
- ? File logging (Production)
- ? Request logging t? ??ng
- ? Structured logging
- ? Enrichers (Machine name, Thread ID, Environment, Application name)

### Option 2: V?i Configuration t? appsettings.json

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Serilog s? t? ??ng ??c t? appsettings.json section "Serilog"
builder.AddBaseNetCoreSerilog();

// ... rest of configuration
```

---

## ?? C?u hình

### appsettings.json Configuration

#### Minimal Configuration (S? d?ng defaults)

Không c?n c?u hình gì - Serilog s? dùng defaults!

#### Full Configuration Example

```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "System": "Warning",
        "BaseNetCore.Core": "Debug"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console",
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/log-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "fileSizeLimitBytes": 10485760,
          "rollOnFileSizeLimit": true,
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"],
    "Properties": {
      "Application": "MyApp"
    }
  },
  "Logging": {
    "FilePath": "logs/log-.txt",
    "RetainedFileCountLimit": 30,
    "FileSizeLimitBytes": 10485760,
    "RollingInterval": "Day"
  }
}
```

### Environment-specific Configuration

**appsettings.Development.json**
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://localhost:5341"
        }
      }
    ]
  }
}
```

**appsettings.Production.json**
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning"
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "/var/log/myapp/log-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 90
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "https://seq.production.com",
          "apiKey": "***"
        }
      }
    ]
  }
}
```

### Log Levels

| Level | Usage | Example |
|-------|-------|---------|
| **Verbose** | R?t chi ti?t, ch? dùng debug | Tracing function calls |
| **Debug** | Development debugging | Variable values, flow control |
| **Information** | Normal flow | "User logged in", "Order created" |
| **Warning** | Unusual but not errors | "Slow query", "Deprecated API" |
| **Error** | Errors and exceptions | Database connection failed |
| **Fatal** | Critical failures | Application crash |

---

## ?? S? d?ng Serilog

### Basic Logging

```csharp
using Microsoft.Extensions.Logging;

public class ProductService
{
    private readonly ILogger<ProductService> _logger;

    public ProductService(ILogger<ProductService> logger)
    {
        _logger = logger;
    }

    public async Task<Product> GetProduct(int id)
    {
        _logger.LogInformation("Getting product with ID: {ProductId}", id);

        var product = await _repository.GetByIdAsync(id);

        if (product == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", id);
            throw new ResourceNotFoundException($"Product {id} not found");
        }

        _logger.LogDebug("Product found: {@Product}", product);
        return product;
    }

    public async Task<int> CreateProduct(Product product)
    {
        try
        {
            _logger.LogInformation("Creating product: {ProductName}", product.Name);
            
            _repository.Add(product);
            var result = await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation("Product created successfully with ID: {ProductId}", product.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product: {ProductName}", product.Name);
            throw;
        }
    }
}
```

### Structured Logging

```csharp
// ? DON'T: String interpolation
_logger.LogInformation($"User {userId} logged in");

// ? DO: Structured logging
_logger.LogInformation("User {UserId} logged in", userId);

// ? BETTER: Log complex objects
_logger.LogInformation("User logged in: {@User}", new 
{ 
    UserId = user.Id, 
    Username = user.Username,
    LoginTime = DateTime.UtcNow
});

// ? BEST: Use destructuring with @
_logger.LogDebug("Processing order: {@Order}", order);
```

### Log Scopes

```csharp
public async Task ProcessOrder(Order order)
{
    using (_logger.BeginScope("OrderId={OrderId}", order.Id))
    {
        _logger.LogInformation("Starting order processing");
        
        // All logs within this scope will include OrderId
        await ValidateOrder(order);
        await CalculateTotal(order);
        await SaveOrder(order);
        
        _logger.LogInformation("Order processing completed");
    }
}
```

---

## ?? Request Logging

### Default Request Logging

BaseNetCore.Core t? ??ng configure request logging v?i:

```csharp
// Middleware pipeline
app.UseBaseNetCoreMiddlewareWithAuth(); // Includes request logging
```

**Log output:**
```
[14:30:45 INF] HTTP GET /api/products responded 200 in 45.2345ms
[14:30:46 WRN] HTTP POST /api/orders responded 400 in 12.5678ms
[14:30:47 ERR] HTTP GET /api/users/999 responded 500 in 123.4567ms
```

### Custom Request Logging

```csharp
app.UseBaseNetCoreSerilogRequestLogging(options =>
{
    // Custom message template
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} from {RequestHost} responded {StatusCode} in {Elapsed:0.0000}ms";

    // Custom log level
    options.GetLevel = (httpContext, elapsed, ex) =>
    {
        if (ex != null) return Serilog.Events.LogEventLevel.Error;
        if (httpContext.Response.StatusCode > 499) return Serilog.Events.LogEventLevel.Error;
        if (httpContext.Response.StatusCode > 399) return Serilog.Events.LogEventLevel.Warning;
        if (elapsed > 5000) return Serilog.Events.LogEventLevel.Warning; // > 5s
        return Serilog.Events.LogEventLevel.Information;
    };

    // Enrich with custom properties
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"]);
        diagnosticContext.Set("CorrelationId", httpContext.TraceIdentifier);
        
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            diagnosticContext.Set("UserId", httpContext.User.FindFirst("userId")?.Value);
        }
    };
});
```

### Exclude Paths t? Request Logging

```csharp
app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (httpContext, elapsed, ex) =>
    {
        // Skip logging for health checks and static files
        if (httpContext.Request.Path.StartsWithSegments("/health") ||
            httpContext.Request.Path.StartsWithSegments("/swagger"))
        {
            return Serilog.Events.LogEventLevel.Verbose; // Won't be logged at default level
        }

        // Normal processing
        if (ex != null) return Serilog.Events.LogEventLevel.Error;
        return Serilog.Events.LogEventLevel.Information;
    };
});
```

---

## ?? Structured Logging

### Log Event Properties

```csharp
// Simple properties
_logger.LogInformation("User {UserId} performed {Action}", userId, "login");

// Complex objects with @ (destructuring)
_logger.LogInformation("Created order: {@Order}", order);

// Multiple properties
_logger.LogInformation(
    "Payment processed: OrderId={OrderId}, Amount={Amount}, Currency={Currency}, PaymentMethod={PaymentMethod}",
    orderId, amount, currency, paymentMethod);
```

### Custom Enrichers

```csharp
// Program.cs
builder.AddBaseNetCoreSerilog(config =>
{
    config.Enrich.WithProperty("Version", "1.0.0");
    config.Enrich.WithProperty("Datacenter", "US-East");
    config.Enrich.With<CustomEnricher>();
});

// Custom enricher class
public class CustomEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty("CustomProperty", "CustomValue"));
    }
}
```

### Context Properties

```csharp
using Serilog.Context;

// Push properties to log context
using (LogContext.PushProperty("TransactionId", transactionId))
{
    _logger.LogInformation("Starting transaction");
    // All logs here will include TransactionId
    await ProcessTransaction();
    _logger.LogInformation("Transaction completed");
}
```

---

## ?? Best Practices

### ? DO

```csharp
// ? Use structured logging
_logger.LogInformation("User {UserId} created order {OrderId}", userId, orderId);

// ? Use appropriate log levels
_logger.LogDebug("Query: {Sql}", sql);
_logger.LogInformation("Order created: {OrderId}", orderId);
_logger.LogWarning("Slow query: {ElapsedMs}ms", elapsed);
_logger.LogError(ex, "Failed to process order: {OrderId}", orderId);

// ? Log exceptions with context
try
{
    // ...
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error processing order {OrderId}", orderId);
    throw;
}

// ? Use scopes for related operations
using (_logger.BeginScope("OrderId={OrderId}", orderId))
{
    _logger.LogInformation("Processing payment");
    _logger.LogInformation("Sending notification");
}

// ? Use @ for complex objects
_logger.LogDebug("User details: {@User}", user);
```

### ? DON'T

```csharp
// ? String interpolation
_logger.LogInformation($"User {userId} logged in");

// ? Concatenation
_logger.LogInformation("User " + userId + " logged in");

// ? Over-logging
_logger.LogInformation("Entering method GetProduct");
_logger.LogInformation("Exiting method GetProduct");

// ? Logging sensitive data
_logger.LogInformation("Password: {Password}", password); // NEVER!
_logger.LogInformation("Credit card: {CardNumber}", cardNumber); // NEVER!

// ? Swallowing exceptions
catch (Exception ex)
{
    _logger.LogError("Error occurred");
    // Lost exception details!
}
```

### Performance Tips

```csharp
// Use LoggerMessage for high-performance logging
private static readonly Action<ILogger, int, Exception?> _logProductNotFound =
    LoggerMessage.Define<int>(
        LogLevel.Warning,
        new EventId(1, nameof(GetProduct)),
        "Product not found: {ProductId}");

public async Task<Product?> GetProduct(int id)
{
    var product = await _repository.GetByIdAsync(id);
    
    if (product == null)
    {
        _logProductNotFound(_logger, id, null);
    }
    
    return product;
}
```

---

## ?? Examples

### Example 1: Simple API Logging

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ILogger<ProductsController> _logger;
    private readonly ProductService _productService;

    public ProductsController(
        ILogger<ProductsController> logger,
        ProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        _logger.LogInformation("Getting product: {ProductId}", id);

        try
        {
            var product = await _productService.GetProductById(id);
            
            if (product == null)
            {
                _logger.LogWarning("Product not found: {ProductId}", id);
                return NotFound();
            }

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product: {ProductId}", id);
            throw;
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        _logger.LogInformation("Creating product: {@Request}", request);

        try
        {
            var product = await _productService.CreateProduct(request);
            
            _logger.LogInformation("Product created: {ProductId}", product.Id);
            
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product: {@Request}", request);
            throw;
        }
    }
}
```

### Example 2: Service v?i Structured Logging

```csharp
public class OrderService : BaseService<Order>
{
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        ILogger<OrderService> logger)
        : base(unitOfWork, httpContextAccessor)
    {
        _logger = logger;
    }

    public async Task<Order> CreateOrder(CreateOrderRequest request)
    {
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["UserId"] = CurrentUserId,
            ["Username"] = CurrentUsername
        }))
        {
            _logger.LogInformation("Creating order: {@Request}", request);

            try
            {
                var order = new Order
                {
                    UserId = CurrentUserId,
                    Items = request.Items,
                    TotalAmount = CalculateTotal(request.Items)
                };

                Repository.Add(order);
                await UnitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Order created successfully: OrderId={OrderId}, TotalAmount={TotalAmount}",
                    order.Id, order.TotalAmount);

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create order: {@Request}", request);
                throw;
            }
        }
    }

    public async Task<Order?> GetOrderWithDetails(int orderId)
    {
        using (_logger.BeginScope("OrderId={OrderId}", orderId))
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var order = await Repository.GetFirstOrDefaultAsync(
                filter: o => o.Id == orderId,
                includeProperties: "Items,User");

            sw.Stop();

            if (sw.ElapsedMilliseconds > 1000)
            {
                _logger.LogWarning("Slow query detected: {ElapsedMs}ms", sw.ElapsedMilliseconds);
            }

            if (order == null)
            {
                _logger.LogWarning("Order not found");
                return null;
            }

            _logger.LogDebug("Order retrieved: {@Order}", order);
            return order;
        }
    }
}
```

### Example 3: File Logging v?i Custom Sinks

```csharp
// Program.cs
builder.AddBaseNetCoreSerilog(config =>
{
    // Write to different files by log level
    config
        .WriteTo.Logger(lc => lc
            .Filter.ByIncludingOnly(evt => evt.Level == Serilog.Events.LogEventLevel.Error)
            .WriteTo.File("logs/errors-.txt", rollingInterval: RollingInterval.Day))
        .WriteTo.Logger(lc => lc
            .Filter.ByIncludingOnly(evt => evt.Level == Serilog.Events.LogEventLevel.Warning)
            .WriteTo.File("logs/warnings-.txt", rollingInterval: RollingInterval.Day))
        .WriteTo.File("logs/all-.txt", rollingInterval: RollingInterval.Day);
});
```

### Example 4: Database Logging

```csharp
// Install package
// dotnet add package Serilog.Sinks.MSSqlServer

// Program.cs
builder.AddBaseNetCoreSerilog(config =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    config.WriteTo.MSSqlServer(
        connectionString: connectionString,
        sinkOptions: new MSSqlServerSinkOptions
        {
            TableName = "Logs",
            SchemaName = "dbo",
            AutoCreateSqlTable = true
        },
        columnOptions: new ColumnOptions
        {
            AdditionalColumns = new Collection<SqlColumn>
            {
                new SqlColumn { ColumnName = "UserId", DataType = SqlDbType.NVarChar, DataLength = 50 },
                new SqlColumn { ColumnName = "Username", DataType = SqlDbType.NVarChar, DataLength = 100 },
            }
        });
});
```

---

## ?? Troubleshooting

### Logs không xu?t hi?n

```csharp
// ??m b?o g?i UseSerilog
builder.Host.UseSerilog();

// Ho?c dùng extension
builder.AddBaseNetCoreSerilog();
```

### Logs không ???c flush khi shutdown

```csharp
try
{
    app.Run();
}
finally
{
    // B?t bu?c ph?i có
    app.FlushBaseNetCoreSerilog();
}
```

### Request logging không ho?t ??ng

```csharp
// Ph?i g?i SAU UseRouting()
app.UseRouting();
app.UseBaseNetCoreSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
```

---

## ?? Related Topics

- [Configuration Guide](../01-Getting-Started/Configuration.md)
- [BaseNetCore Extensions](BaseNetCore-Extensions.md)
- [Global Exception Middleware](../06-Exception-Handling/Global-Exception-Middleware.md)
- [Performance Optimization](../12-Best-Practices/Performance-Optimization.md)

---

## ?? External Resources

- [Serilog Documentation](https://serilog.net/)
- [Serilog Best Practices](https://github.com/serilog/serilog/wiki/Best-Practices)
- [ASP.NET Core Logging](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/)

---

**[? Back to Documentation](../README.md)**
