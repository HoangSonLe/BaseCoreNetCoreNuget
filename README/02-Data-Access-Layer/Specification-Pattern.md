# ?? Specification Pattern

## M?c l?c
- [Gi?i thi?u](#gi?i-thi?u)
- [Implementation](#implementation)
- [Usage Examples](#usage-examples)
- [Best Practices](#best-practices)

---

## ?? Gi?i thi?u

**Specification Pattern** encapsulate query logic thành reusable components.

### Gi?i quy?t v?n ?? gì?

```csharp
// ? PROBLEM - Query logic scattered everywhere
public async Task<List<Product>> GetActiveProducts()
{
    return await _repo.GetAllAsync(p => p.IsActive && p.Stock > 0);
}

public async Task<List<Product>> GetActiveProductsByCategory(string category)
{
    return await _repo.GetAllAsync(p => p.IsActive && p.Stock > 0 && p.Category == category);
}
```

```csharp
// ? SOLUTION - Reusable specification
public class ActiveProductsSpec : BaseSpecification<Product>
{
    public ActiveProductsSpec()
    {
        WithCriteria(p => p.IsActive && p.Stock > 0);
    }
}

var spec = new ActiveProductsSpec();
var products = await _repo.GetAsync(spec);
```

---

## ?? Interface

```csharp
public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    Expression<Func<T, object>> OrderBy { get; }
    Expression<Func<T, object>> OrderByDescending { get; }
    int Skip { get; }
    int Take { get; }
    bool IsPagingEnabled { get; }
    bool AsNoTracking { get; }
}
```

---

## ?? Implementation

### BaseSpecification<T>

```csharp
public class BaseSpecification<T> : ISpecification<T>
{
    public Expression<Func<T, bool>> Criteria { get; private set; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public Expression<Func<T, object>> OrderBy { get; private set; }
    public Expression<Func<T, object>> OrderByDescending { get; private set; }
    public int Skip { get; private set; }
    public int Take { get; private set; }
    public bool IsPagingEnabled { get; private set; }
    public bool AsNoTracking { get; private set; } = true;

    // Fluent API
    public BaseSpecification<T> WithCriteria(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
   return this;
    }

    public BaseSpecification<T> AndCriteria(Expression<Func<T, bool>> criteria)
    {
        if (Criteria == null)
  {
            Criteria = criteria;
        }
        else
 {
     var parameter = Expression.Parameter(typeof(T));
            var combined = Expression.AndAlso(
    Expression.Invoke(Criteria, parameter),
                Expression.Invoke(criteria, parameter));
          Criteria = Expression.Lambda<Func<T, bool>>(combined, parameter);
        }
        return this;
    }

    public BaseSpecification<T> WithInclude(Expression<Func<T, object>> include)
    {
Includes.Add(include);
        return this;
    }

    public BaseSpecification<T> WithOrderBy(Expression<Func<T, object>> orderBy)
    {
        OrderBy = orderBy;
     return this;
    }

    public BaseSpecification<T> WithOrderByDescending(Expression<Func<T, object>> orderBy)
    {
        OrderByDescending = orderBy;
        return this;
    }

    public BaseSpecification<T> WithPagedResults(int pageNumber, int pageSize)
    {
      Skip = (pageNumber - 1) * pageSize;
 Take = pageSize;
      IsPagingEnabled = true;
        return this;
    }

    public BaseSpecification<T> WithPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
        return this;
    }

    public BaseSpecification<T> WithTracking(bool tracking = true)
    {
        AsNoTracking = !tracking;
        return this;
    }
}
```

---

## ?? Usage Examples

### Example 1: Named Specifications

```csharp
// Specification cho active products
public class ActiveProductsSpec : BaseSpecification<Product>
{
    public ActiveProductsSpec()
    {
        WithCriteria(p => p.IsActive);
        WithOrderBy(p => p.Name);
    }
}

// Usage
var spec = new ActiveProductsSpec();
var products = await _productRepo.GetAsync(spec);
```

### Example 2: Parameterized Specifications

```csharp
public class ProductsByCategorySpec : BaseSpecification<Product>
{
    public ProductsByCategorySpec(string category, decimal? minPrice = null)
    {
        WithCriteria(p => p.IsActive && p.Category == category);

        if (minPrice.HasValue)
{
            AndCriteria(p => p.Price >= minPrice.Value);
     }

        WithOrderBy(p => p.Price);
    }
}

// Usage
var spec = new ProductsByCategorySpec("Smartphone", minPrice: 10000000);
var products = await _productRepo.GetAsync(spec);
```

### Example 3: Specifications with Includes

```csharp
public class OrderWithDetailsSpec : BaseSpecification<Order>
{
    public OrderWithDetailsSpec(int orderId)
 {
WithCriteria(o => o.Id == orderId);
        WithInclude(o => o.Customer);
        WithInclude(o => o.OrderItems);
   WithInclude(o => o.ShippingAddress);
    }
}

// Usage
var spec = new OrderWithDetailsSpec(orderId);
var order = await _orderRepo.FirstOrDefaultAsync(spec);
```

### Example 4: Complex Search Specification

```csharp
public class ProductSearchSpec : BaseSpecification<Product>
{
    public ProductSearchSpec(
        string keyword = null,
        string category = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int pageNumber = 1,
    int pageSize = 20)
    {
  // Base criteria - active products
   WithCriteria(p => p.IsActive);

        // Keyword search
  if (!string.IsNullOrWhiteSpace(keyword))
        {
          var normalized = SearchFieldUtils.NormalizeSearchText(keyword);
            AndCriteria(p => p.NonUnicodeSearchString.Contains(normalized));
        }

        // Category filter
        if (!string.IsNullOrWhiteSpace(category))
  {
            AndCriteria(p => p.Category == category);
        }

        // Price range
      if (minPrice.HasValue)
        {
            AndCriteria(p => p.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
 {
          AndCriteria(p => p.Price <= maxPrice.Value);
        }

        // Sorting and paging
        WithOrderBy(p => p.Name);
        WithPagedResults(pageNumber, pageSize);
    }
}

// Usage
var spec = new ProductSearchSpec(
    keyword: "iphone",
    category: "Smartphone",
    minPrice: 20000000,
    pageNumber: 1,
    pageSize: 20
);

var result = await _productRepo.GetWithPagingAsync(spec);
```

### Example 5: Combining Specifications

```csharp
public class CompositeProductSpec : BaseSpecification<Product>
{
    public CompositeProductSpec(bool includeCategory = false, bool includeReviews = false)
    {
     WithCriteria(p => p.IsActive);

      if (includeCategory)
{
   WithInclude(p => p.Category);
        }

      if (includeReviews)
        {
   WithInclude(p => p.Reviews);
}

        WithOrderByDescending(p => p.CreatedDate);
WithPagedResults(1, 10);
    }
}
```

---

## ?? Best Practices

### 1. Single Responsibility

```csharp
// ? GOOD - Each spec has single purpose
public class ActiveProductsSpec : BaseSpecification<Product>
{
 public ActiveProductsSpec()
    {
  WithCriteria(p => p.IsActive);
    }
}

public class LowStockSpec : BaseSpecification<Product>
{
    public LowStockSpec(int threshold)
    {
        WithCriteria(p => p.Stock <= threshold);
    }
}

// Combine
var activeSpec = new ActiveProductsSpec();
var lowStockSpec = new LowStockSpec(10);

var spec = new BaseSpecification<Product>()
    .WithCriteria(activeSpec.Criteria)
    .AndCriteria(lowStockSpec.Criteria);
```

### 2. Reusable Base Specs

```csharp
public class ActiveEntitySpec<T> : BaseSpecification<T> where T : BaseEntity
{
    public ActiveEntitySpec()
    {
        WithCriteria(e => e.IsActive);
    }
}

// Reuse
var activeProducts = new ActiveEntitySpec<Product>();
var activeCategories = new ActiveEntitySpec<Category>();
```

### 3. Avoid Over-Engineering

```csharp
// ? BAD - Too specific
public class ProductWithNameStartingWithIAndPriceGreaterThan1000Spec : BaseSpecification<Product>
{
    public ProductWithNameStartingWithIAndPriceGreaterThan1000Spec() { }
}

// ? GOOD - Flexible parameters
public class ProductFilterSpec : BaseSpecification<Product>
{
    public ProductFilterSpec(string namePrefix, decimal minPrice)
    {
  WithCriteria(p => p.Name.StartsWith(namePrefix) && p.Price >= minPrice);
    }
}
```

---

## ?? Related Topics

- [Repository Pattern](Repository-Pattern.md)
- [Query Object Pattern](Query-Object-Pattern.md)
- [Performance Optimization](../12-Best-Practices/Performance-Optimization.md)

---

**[? Back to Documentation](../README.md)**
