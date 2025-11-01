# ?? Search Example

## Vietnamese Search Implementation

Complete example với Product search.

---

## Entity

```csharp
[SearchableEntity]
public class Product : BaseSearchableEntity
{
    public int Id { get; set; }

    [SearchableField(Order = 1)]
    public string Name { get; set; }

    [SearchableField(Order = 2)]
    public string Code { get; set; }

    [SearchableField(Order = 3)]
 public string Brand { get; set; }

[SearchableField(Order = 4)]
 public string Description { get; set; }

 public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; }
}
```

---

## Search Service

```csharp
public interface IProductSearchService
{
    Task<PageResponse<Product>> SearchAsync(ProductSearchQuery query);
    Task<List<string>> GetSuggestionsAsync(string keyword);
}

public class ProductSearchService : IProductSearchService
{
    private readonly IRepository<Product> _productRepo;

    public async Task<PageResponse<Product>> SearchAsync(ProductSearchQuery query)
    {
        var spec = new BaseSpecification<Product>();

// Base filter - active products
        spec.WithCriteria(p => p.IsActive);

        // Keyword search
   if (!string.IsNullOrWhiteSpace(query.Keyword))
  {
    var normalized = SearchFieldUtils.NormalizeSearchText(query.Keyword);
       spec.AndCriteria(p => p.NonUnicodeSearchString.Contains(normalized));
        }

 // Brand filter
 if (!string.IsNullOrWhiteSpace(query.Brand))
        {
            spec.AndCriteria(p => p.Brand == query.Brand);
        }

   // Price range
        if (query.MinPrice.HasValue)
     spec.AndCriteria(p => p.Price >= query.MinPrice.Value);

        if (query.MaxPrice.HasValue)
   spec.AndCriteria(p => p.Price <= query.MaxPrice.Value);

// Stock filter
        if (query.InStockOnly)
 spec.AndCriteria(p => p.Stock > 0);

        // Sorting
     switch (query.SortBy?.ToLower())
     {
       case "price_asc":
          spec.WithOrderBy(p => p.Price);
     break;
            case "price_desc":
       spec.WithOrderByDescending(p => p.Price);
        break;
          case "name":
       spec.WithOrderBy(p => p.Name);
     break;
  default:
           spec.WithOrderByDescending(p => p.CreatedAt);
  break;
        }

  // Pagination
        spec.WithPagedResults(query.Page, query.Size);

return await _productRepo.GetWithPagingAsync(spec);
    }

    public async Task<List<string>> GetSuggestionsAsync(string keyword)
    {
   if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
    return new List<string>();

var normalized = SearchFieldUtils.NormalizeSearchText(keyword);

        var spec = new BaseSpecification<Product>()
    .WithCriteria(p => p.IsActive && p.NonUnicodeSearchString.Contains(normalized))
   .WithOrderBy(p => p.Name)
 .WithPaging(0, 10);

        var products = await _productRepo.GetAsync(spec);
        return products.Select(p => p.Name).ToList();
    }
}

public class ProductSearchQuery
{
    public string Keyword { get; set; }
    public string Brand { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool InStockOnly { get; set; }
    public string SortBy { get; set; }
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 20;
}
```

---

## Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IProductSearchService _searchService;

    [HttpGet("products")]
    public async Task<IActionResult> SearchProducts([FromQuery] ProductSearchQuery query)
    {
  var result = await _searchService.SearchAsync(query);
return Ok(result);
    }

    [HttpGet("suggestions")]
    public async Task<IActionResult> GetSuggestions([FromQuery] string keyword)
    {
  var suggestions = await _searchService.GetSuggestionsAsync(keyword);
     return Ok(suggestions);
    }
}
```

---

## Test Cases

### Search: "iphone"

```http
GET /api/search/products?keyword=iphone&page=1&size=20
```

**Matches:**
- "iPhone 15 Pro Max"
- "?i?n tho?i iPhone 14"
- "Case iPhone"

### Search: "dien thoai" (no diacritics)

```http
GET /api/search/products?keyword=dien thoai
```

**Matches:**
- "?i?n tho?i iPhone"
- "?i?n tho?i Samsung"

### Search with filters

```http
GET /api/search/products?keyword=samsung&minPrice=10000000&maxPrice=30000000&inStockOnly=true&sortBy=price_asc
```

### Autocomplete

```http
GET /api/search/suggestions?keyword=ip
```

**Response:**
```json
[
  "iPhone 15 Pro Max",
  "iPhone 14 Pro",
  "iPad Air 5"
]
```

---

**[? Back to Documentation](../README.md)**
