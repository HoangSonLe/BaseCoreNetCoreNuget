# ?? Search Best Practices

## Khi nào nên dùng Searchable Fields?

### ? Dùng cho

- **Products/Items** - Tên, mã, mô t?
- **Customers** - Tên, email, s? ?i?n tho?i, ??a ch?
- **Employees** - Tên, phòng ban, ch?c v?
- **Documents** - Tiêu ??, n?i dung, tags
- **Master Data** - Danh m?c, categories

### ? Không dùng cho

- **Transactional Data** - Orders, payments, logs
- **High-volume Data** - Millions of records inserted daily
- **Exact Match Only** - ID lookups, code lookups
- **Structured Queries** - Date ranges, numeric filters

---

## Performance Optimization

### 1. Database Index

```csharp
// Migration
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>(entity =>
    {
     // ? Add index
        entity.HasIndex(e => e.NonUnicodeSearchString);
    
   // ? Or filtered index (SQL Server)
     // CREATE INDEX IX_Products_Search 
 // ON Products(NonUnicodeSearchString)
      // WHERE IsActive = 1;
    });
}
```

### 2. Pagination

```csharp
// ? Always paginate search results
var spec = new ProductSearchSpec(keyword)
    .WithPagedResults(pageNumber, pageSize);
```

### 3. Caching

```csharp
public class CachedProductService
{
    private readonly IMemoryCache _cache;

    public async Task<List<Product>> SearchProducts(string keyword)
    {
  var cacheKey = $"product:search:{keyword}";
        
if (_cache.TryGetValue(cacheKey, out List<Product> cached))
      return cached;

        var results = await _productRepo.GetAllAsync(...);
      
 _cache.Set(cacheKey, results, TimeSpan.FromMinutes(5));
        return results;
    }
}
```

### 4. Asynchronous

```csharp
// ? Always use async
await SearchProductsAsync(keyword);

// ? Don't block
var result = SearchProductsAsync(keyword).Result;
```

---

## Security Best Practices

### 1. Prevent SQL Injection

```csharp
// ? SAFE - EF Core parameterizes queries
var normalized = SearchFieldUtils.NormalizeSearchText(keyword);
var products = await _repo.GetAllAsync(
 filter: p => p.NonUnicodeSearchString.Contains(normalized)
);

// ? UNSAFE - Raw SQL
var sql = $"SELECT * FROM Products WHERE NonUnicodeSearchString LIKE '%{keyword}%'";
```

### 2. Input Validation

```csharp
public async Task<List<Product>> SearchProducts(string keyword)
{
    // ? Validate input
    if (string.IsNullOrWhiteSpace(keyword))
        return new List<Product>();
        
    if (keyword.Length < 2)
    throw new BadRequestException("Search keyword must be at least 2 characters");
    
    if (keyword.Length > 100)
        throw new BadRequestException("Search keyword too long");
  
    // Continue with search...
}
```

### 3. Rate Limiting

```csharp
[RateLimit(PermitLimit = 10, Window = 60)] // 10 requests per minute
[HttpGet("search")]
public async Task<IActionResult> Search([FromQuery] string keyword)
{
 // ...
}
```

---

## UI/UX Best Practices

### 1. Debounce User Input

```javascript
// Frontend debounce
let searchTimeout;
function onSearchInput(keyword) {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
  searchProducts(keyword);
    }, 300); // Wait 300ms after user stops typing
}
```

### 2. Minimum Keyword Length

```csharp
// Backend
if (keyword.Length < 2)
    return new List<Product>();

// Frontend
<input 
    type="search" 
  minlength="2"
    placeholder="Enter at least 2 characters..."
/>
```

### 3. Highlight Results

```javascript
// Frontend
function highlightKeyword(text, keyword) {
    const regex = new RegExp(`(${keyword})`, 'gi');
    return text.replace(regex, '<mark>$1</mark>');
}
```

---

## Testing

### Unit Tests

```csharp
[Test]
public void SearchFieldUtils_RemoveVietnameseDiacritics_Success()
{
    // Arrange
    var input = "?i?n tho?i Nguy?n V?n An";
    
    // Act
    var result = SearchFieldUtils.RemoveVietnameseDiacritics(input);
    
  // Assert
  Assert.AreEqual("Dien thoai Nguyen Van An", result);
}

[Test]
public async Task SearchProducts_WithKeyword_ReturnsMatchingProducts()
{
    // Arrange
    var keyword = "iphone";
    
    // Act
 var results = await _productService.SearchProducts(keyword);
    
    // Assert
    Assert.IsTrue(results.All(p => 
        p.NonUnicodeSearchString.Contains("iphone")));
}
```

---

## Common Pitfalls

### ? Không normalize keyword

```csharp
// ? BAD
var products = await _repo.GetAllAsync(
    filter: p => p.NonUnicodeSearchString.Contains(keyword)  // Raw keyword!
);

// ? GOOD
var normalized = SearchFieldUtils.NormalizeSearchText(keyword);
var products = await _repo.GetAllAsync(
    filter: p => p.NonUnicodeSearchString.Contains(normalized)
);
```

### ? Quên index database

```csharp
// ? Missing index ? Slow queries
entity.Property(e => e.NonUnicodeSearchString).HasMaxLength(2000);

// ? With index ? Fast queries
entity.Property(e => e.NonUnicodeSearchString).HasMaxLength(2000);
entity.HasIndex(e => e.NonUnicodeSearchString);
```

### ? Search trên entities không có [SearchableEntity]

```csharp
// ? Order không có [SearchableEntity]
public class Order : BaseAuditableEntity
{
    // No NonUnicodeSearchString property!
}

// ? This will fail
var orders = await _orderRepo.GetAllAsync(
  filter: o => o.NonUnicodeSearchString.Contains(keyword)  // Compile error!
);
```

---

**[? Back to Documentation](../README.md)**
