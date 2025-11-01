# ?? Advanced Scenarios

## 1. Multi-tenancy

```csharp
public interface ITenantEntity
{
    int TenantId { get; set; }
}

[SearchableEntity]
public class Product : BaseSearchableEntity, ITenantEntity
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    
    [SearchableField(Order = 1)]
    public string Name { get; set; }
}

public class TenantService<TEntity> : BaseService<TEntity>
    where TEntity : BaseAuditableEntity, ITenantEntity
{
  protected int TenantId => int.Parse(CurrentUser?.FindFirst("tenant_id")?.Value ?? "0");

    public async Task<List<TEntity>> GetMyDataAsync()
    {
        var spec = new BaseSpecification<TEntity>()
   .WithCriteria(e => e.TenantId == TenantId);

        return await Repository.GetAsync(spec);
    }
}
```

---

## 2. Soft Delete

```csharp
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
}

public class SoftDeleteService<TEntity> : BaseService<TEntity>
    where TEntity : BaseAuditableEntity, ISoftDeletable
{
    public async Task SoftDeleteAsync(int id)
    {
        var entity = await Repository.GetByIdAsync(id);
        if (entity == null)
        throw new ResourceNotFoundException($"Entity {id} not found");

     entity.IsDeleted = true;
 entity.DeletedAt = DateTime.UtcNow;
        
        Repository.Update(entity);
        await UnitOfWork.SaveChangesAsync();
    }

    public async Task<List<TEntity>> GetActiveAsync()
    {
   var spec = new BaseSpecification<TEntity>()
      .WithCriteria(e => !e.IsDeleted);

        return await Repository.GetAsync(spec);
 }
}
```

---

## 3. Audit Logging

```csharp
public class AuditLog
{
    public int Id { get; set; }
    public string EntityName { get; set; }
    public int EntityId { get; set; }
    public string Action { get; set; }
    public string Changes { get; set; }
    public int UserId { get; set; }
    public DateTime Timestamp { get; set; }
}

public class AuditableUnitOfWork : UnitOfWork
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
   var entries = _context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);

      var auditLogs = new List<AuditLog>();

     foreach (var entry in entries)
        {
            auditLogs.Add(new AuditLog
      {
     EntityName = entry.Entity.GetType().Name,
                Action = entry.State.ToString(),
  Changes = JsonSerializer.Serialize(entry.CurrentValues.Properties),
     UserId = GetCurrentUserId(),
      Timestamp = DateTime.UtcNow
            });
 }

    var result = await base.SaveChangesAsync(cancellationToken);

// Save audit logs
        _context.Set<AuditLog>().AddRange(auditLogs);
        await _context.SaveChangesAsync(cancellationToken);

        return result;
    }
}
```

---

## 4. Caching with Redis

```bash
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
```

```csharp
public class CachedProductService : IProductService
{
    private readonly IProductService _inner;
    private readonly IDistributedCache _cache;

    public async Task<Product> GetByIdAsync(int id)
    {
        var cacheKey = $"product:{id}";
   
        var cached = await _cache.GetStringAsync(cacheKey);
        if (cached != null)
            return JsonSerializer.Deserialize<Product>(cached);

        var product = await _inner.GetByIdAsync(id);
        
        await _cache.SetStringAsync(cacheKey, 
         JsonSerializer.Serialize(product),
            new DistributedCacheEntryOptions
 {
         AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
         });

      return product;
    }
}
```

---

## 5. Event Sourcing

```csharp
public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}

public class ProductCreatedEvent : IDomainEvent
{
    public int ProductId { get; set; }
    public string Name { get; set; }
    public DateTime OccurredOn { get; set; }
}

public class ProductService : BaseService<Product>
{
    private readonly IEventBus _eventBus;

    public async Task<Product> CreateAsync(Product product)
    {
  Repository.Add(product);
        await UnitOfWork.SaveChangesAsync();

        await _eventBus.PublishAsync(new ProductCreatedEvent
        {
  ProductId = product.Id,
          Name = product.Name,
     OccurredOn = DateTime.UtcNow
        });

 return product;
    }
}
```

---

## 6. Background Jobs with Hangfire

```bash
dotnet add package Hangfire
dotnet add package Hangfire.SqlServer
```

```csharp
// Program.cs
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

// Service
public class ProductService
{
    [Queue("default")]
    public async Task ProcessBulkImportAsync(List<Product> products)
    {
foreach (var product in products)
     {
  Repository.Add(product);
        }
    await UnitOfWork.SaveChangesAsync();
    }
}

// Usage
BackgroundJob.Enqueue<ProductService>(x => 
 x.ProcessBulkImportAsync(products));
```

---

**[? Back to Documentation](../README.md)**
