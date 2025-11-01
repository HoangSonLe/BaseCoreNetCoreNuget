# ???? Vietnamese Search

## T?i sao c?n Vietnamese Search?

```csharp
// ? Problem: Standard search không tìm ???c
var products = db.Products.Where(p => p.Name.Contains("dien thoai"));
// ? Returns EMPTY (vì database có "?i?n tho?i")

// ? Solution: Vietnamese search
var normalized = SearchFieldUtils.NormalizeSearchText("dien thoai");
var products = db.Products.Where(p => p.NonUnicodeSearchString.Contains(normalized));
// ? Returns products with "?i?n tho?i" ?
```

---

## How It Works

### 1. Auto-generate Search String

```csharp
var product = new Product
{
    Name = "?i?n tho?i iPhone 15 Pro Max",
 Code = "IP15-PM-256",
 Brand = "Apple"
};

// When saved ? NonUnicodeSearchString auto-generated:
// "dien thoai iphone 15 pro max ip15-pm-256 apple"
```

### 2. Normalize Search Keyword

```csharp
public async Task<List<Product>> SearchProducts(string keyword)
{
    // Input: "?i?n tho?i" or "dien thoai"
    var normalized = SearchFieldUtils.NormalizeSearchText(keyword);
    // Output: "dien thoai"
    
  return await _repo.GetAllAsync(
 filter: p => p.NonUnicodeSearchString.Contains(normalized)
    );
}
```

---

## SearchFieldUtils Methods

### RemoveVietnameseDiacritics

```csharp
var input = "Nguy?n V?n An - ?i?n tho?i iPhone";
var output = SearchFieldUtils.RemoveVietnameseDiacritics(input);
// ? "Nguyen Van An - Dien thoai iPhone"
```

### NormalizeSearchText

```csharp
var input = "?i?n Tho?i  IPHONE  15  ";
var output = SearchFieldUtils.NormalizeSearchText(input);
// ? "dien thoai iphone 15" (lowercase, trimmed, single spaces)
```

### BuildString

```csharp
[SearchableEntity]
public class Product : BaseSearchableEntity
{
    [SearchableField(Order = 1)]
    public string Name { get; set; }
    
    [SearchableField(Order = 2)]
    public string Code { get; set; }
}

var product = new Product
{
    Name = "?i?n tho?i Samsung",
Code = "SGS24"
};

var searchString = SearchFieldUtils.BuildString(product);
// ? "dien thoai samsung sgs24"
```

---

## Search Examples

### Simple Search

```csharp
public async Task<List<Product>> SearchProducts(string keyword)
{
    if (string.IsNullOrWhiteSpace(keyword))
        return new List<Product>();

 var normalized = SearchFieldUtils.NormalizeSearchText(keyword);

    return await _productRepo.GetAllAsync(
        filter: p => p.NonUnicodeSearchString.Contains(normalized),
  tracking: false
    );
}

// All these work:
await SearchProducts("iphone");  // ?
await SearchProducts("iPhone");  // ?
await SearchProducts("IPHONE");  // ?
await SearchProducts("?i?n tho?i");  // ?
await SearchProducts("dien thoai");  // ?
```

### Search with Pagination

```csharp
public async Task<PageResponse<Product>> SearchProductsWithPaging(
  string keyword, 
    int pageNumber, 
    int pageSize)
{
    var normalized = SearchFieldUtils.NormalizeSearchText(keyword);
    
    var spec = new BaseSpecification<Product>()
        .WithCriteria(p => p.NonUnicodeSearchString.Contains(normalized))
   .WithOrderBy(p => p.Name)
        .WithPagedResults(pageNumber, pageSize);

  return await _productRepo.GetWithPagingAsync(spec);
}
```

### Multi-keyword Search (AND)

```csharp
public async Task<List<Product>> SearchByAllKeywords(params string[] keywords)
{
    var normalizedKeywords = keywords
        .Select(k => SearchFieldUtils.NormalizeSearchText(k))
      .Where(k => !string.IsNullOrEmpty(k))
     .ToArray();

    var spec = new BaseSpecification<Product>();
    
    foreach (var keyword in normalizedKeywords)
    {
        spec.AndCriteria(p => p.NonUnicodeSearchString.Contains(keyword));
    }

    return await _productRepo.GetAsync(spec);
}

// Usage: Find products containing ALL keywords
await SearchByAllKeywords("iphone", "15", "pro");
// ? Only products with "iphone", "15", AND "pro" in search string
```

### Multi-keyword Search (OR)

```csharp
public async Task<List<Product>> SearchByAnyKeyword(params string[] keywords)
{
    var normalizedKeywords = keywords
        .Select(k => SearchFieldUtils.NormalizeSearchText(k))
        .Where(k => !string.IsNullOrEmpty(k))
   .ToArray();

    if (normalizedKeywords.Length == 0)
        return new List<Product>();

    var spec = new BaseSpecification<Product>()
 .WithCriteria(p => p.NonUnicodeSearchString.Contains(normalizedKeywords[0]));

    for (int i = 1; i < normalizedKeywords.Length; i++)
    {
        var keyword = normalizedKeywords[i];
        spec.OrCriteria(p => p.NonUnicodeSearchString.Contains(keyword));
    }

    return await _productRepo.GetAsync(spec);
}

// Usage: Find products containing ANY keyword
await SearchByAnyKeyword("iphone", "samsung", "xiaomi");
// ? Products with "iphone" OR "samsung" OR "xiaomi"
```

### Autocomplete Suggestions

```csharp
public async Task<List<string>> GetSuggestions(string keyword)
{
    if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
   return new List<string>();

    var normalized = SearchFieldUtils.NormalizeSearchText(keyword);

    var spec = new BaseSpecification<Product>()
        .WithCriteria(p => p.NonUnicodeSearchString.Contains(normalized))
        .WithOrderBy(p => p.Name)
        .WithPaging(0, 10);

    var products = await _productRepo.GetAsync(spec);
    
return products.Select(p => p.Name).ToList();
}
```

---

## Database Setup

### Add NonUnicodeSearchString Column

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>(entity =>
    {
   entity.Property(e => e.NonUnicodeSearchString)
        .HasMaxLength(2000);
   
        // ? Add index for faster search
        entity.HasIndex(e => e.NonUnicodeSearchString);
    });
}
```

### Migration

```sql
ALTER TABLE Products
ADD NonUnicodeSearchString NVARCHAR(2000);

CREATE INDEX IX_Products_NonUnicodeSearchString
ON Products(NonUnicodeSearchString);
```

---

## Performance Tips

### 1. Add Database Index

```csharp
entity.HasIndex(e => e.NonUnicodeSearchString);
```

### 2. Use AsNoTracking

```csharp
await _repo.GetAllAsync(
    filter: p => p.NonUnicodeSearchString.Contains(normalized),
    tracking: false  // ? Faster for read-only
);
```

### 3. Limit Results

```csharp
var spec = new BaseSpecification<Product>()
    .WithCriteria(p => p.NonUnicodeSearchString.Contains(normalized))
    .WithPaging(0, 100);  // ? Limit results
```

### 4. Cache Frequent Searches

```csharp
private readonly IMemoryCache _cache;

public async Task<List<Product>> SearchProducts(string keyword)
{
    var cacheKey = $"search:{keyword}";
    
    if (!_cache.TryGetValue(cacheKey, out List<Product> products))
    {
      products = await _productRepo.GetAllAsync(...);
      
    _cache.Set(cacheKey, products, TimeSpan.FromMinutes(5));
    }
    
    return products;
}
```

---

## Supported Characters

### Vietnamese Diacritics

```
á à ? ã ? ? a
? ? ? ? ? ? ? a
â ? ? ? ? ? ? a
é è ? ? ? ? e
ê ? ? ? ? ? ? e
í ì ? ? ? ? i
ó ò ? õ ? ? o
ô ? ? ? ? ? ? o
? ? ? ? ? ? ? o
ú ù ? ? ? ? u
? ? ? ? ? ? ? u
ý ? ? ? ? ? y
? ? d
```

---

**[? Back to Documentation](../README.md)**
