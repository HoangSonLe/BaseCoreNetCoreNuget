# ?? Query Object Pattern

## Giới thiệu

**Query Object Pattern** là alternative cho Specification Pattern, cung c?p **fluent interface** ?? xây d?ng queries.

---

## Interface

```csharp
public interface IQuery<TEntity> where TEntity : class
{
    Expression<Func<TEntity, bool>> Filter { get; set; }
    Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> OrderBy { get; set; }
    PageRequest Paging { get; set; }
List<Expression<Func<TEntity, object>>> Includes { get; set; }
    bool AsNoTracking { get; set; }
}
```

---

## Implementation

```csharp
public class Query<TEntity> : IQuery<TEntity> where TEntity : class
{
    public Expression<Func<TEntity, bool>> Filter { get; set; }
  public Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> OrderBy { get; set; }
    public PageRequest Paging { get; set; }
    public List<Expression<Func<TEntity, object>>> Includes { get; set; }
    public bool AsNoTracking { get; set; }

    public Query()
    {
        Includes = new List<Expression<Func<TEntity, object>>>();
        AsNoTracking = true;
    }

    // Fluent API
    public Query<TEntity> WithFilter(Expression<Func<TEntity, bool>> filter)
    {
        Filter = filter;
        return this;
    }

    public Query<TEntity> WithOrderBy(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy)
    {
        OrderBy = orderBy;
     return this;
    }

    public Query<TEntity> WithPaging(int pageNumber, int pageSize)
    {
  Paging = new PageRequest(pageNumber, pageSize);
        return this;
    }

    public Query<TEntity> WithInclude(Expression<Func<TEntity, object>> include)
 {
        Includes.Add(include);
        return this;
    }

    public Query<TEntity> WithTracking(bool tracking = true)
    {
        AsNoTracking = !tracking;
        return this;
    }
}
```

---

## Usage Examples

### Basic Query

```csharp
var query = new Query<Product>()
    .WithFilter(p => p.IsActive)
    .WithOrderBy(q => q.OrderBy(p => p.Name))
    .WithPaging(1, 20);

var products = await ExecuteQueryAsync(query);
```

### Complex Query với Multiple Conditions

```csharp
var query = new Query<Product>()
    .WithFilter(p => p.IsActive && p.Category == "Smartphone" && p.Price >= 10000000)
    .WithInclude(p => p.Category)
    .WithInclude(p => p.Reviews)
    .WithOrderBy(q => q.OrderByDescending(p => p.CreatedDate).ThenBy(p => p.Name))
    .WithPaging(pageNumber, pageSize)
    .WithTracking(false);

var products = await ExecuteQueryAsync(query);
```

---

## Specification vs Query Object

| Feature | Specification | Query Object |
|---------|---------------|--------------|
| **Type Safety** | ? Strong | ?? Moderate |
| **Reusability** | ? High | ?? Medium |
| **Flexibility** | ?? Medium | ? High |
| **Complexity** | ?? Medium | ?? Low |
| **Best for** | Complex business rules | Ad-hoc queries |

---

**[? Back to Documentation](../README.md)**
