# ? BaseNetCore Extensions

## AddBaseNetCoreFeatures

Adds all core features **without authentication**.

```csharp
builder.Services.AddBaseNetCoreFeatures(builder.Configuration);
```

**Includes:**
- ? Automatic model validation
- ? Base service dependencies (HttpContextAccessor)
- ? AES encryption configuration
- ? Controllers with JSON options

---

## AddBaseNetCoreFeaturesWithAuth

Adds all features **with JWT authentication**.

```csharp
builder.Services.AddBaseNetCoreFeaturesWithAuth(builder.Configuration);
```

**Includes:**
- ? Everything from `AddBaseNetCoreFeatures`
- ? JWT authentication
- ? Token service
- ? Memory cache
- ? Auto-register `ITokenValidator`

---

## UseBaseNetCoreMiddleware

Adds core middleware **without authentication**.

```csharp
app.UseBaseNetCoreMiddleware();
```

**Includes:**
- ? GlobalExceptionMiddleware

---

## UseBaseNetCoreMiddlewareWithAuth

Adds middleware **with authentication**.

```csharp
app.UseBaseNetCoreMiddlewareWithAuth();
```

**Includes:**
- ? GlobalExceptionMiddleware
- ? UseAuthentication()
- ? UseAuthorization()

---

## Complete Example

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add BaseNetCore with auth
builder.Services.AddBaseNetCoreFeaturesWithAuth(builder.Configuration);

// Add UnitOfWork
builder.Services.AddScoped<IUnitOfWork>(provider =>
{
    var dbContext = provider.GetRequiredService<ApplicationDbContext>();
    return new UnitOfWork(dbContext);
});

var app = builder.Build();

app.UseRouting();
app.UseBaseNetCoreMiddlewareWithAuth();
app.MapControllers();
app.Run();
```

---

**[? Back to Documentation](../README.md)**
