# ?? DI Utilities

## AddAutoRegisterDI<T>

Auto-register implementation c?a interface t? all assemblies.

---

## Usage

```csharp
// Auto-register ITokenValidator
builder.Services.AddAutoRegisterDI<ITokenValidator>();
```

---

## How It Works

```csharp
public static class DIUtils
{
    public static IServiceCollection AddAutoRegisterDI<T>(this IServiceCollection services)
{
        var interfaceType = typeof(T);
        
        var implType = AppDomain.CurrentDomain.GetAssemblies()
   .SelectMany(GetLoadableTypes)
            .Where(t => t.IsClass && !t.IsAbstract && interfaceType.IsAssignableFrom(t))
 .FirstOrDefault();

 if (implType != null)
        {
    services.TryAddScoped(interfaceType, implType);
}

        return services;
}
}
```

---

## Examples

```csharp
// Token validator
builder.Services.AddAutoRegisterDI<ITokenValidator>();

// Custom service
builder.Services.AddAutoRegisterDI<ICustomService>();
```

---

**[? Back to Documentation](../README.md)**
