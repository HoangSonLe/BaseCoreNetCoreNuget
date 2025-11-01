# ?? Pagination

## PageRequest

```csharp
public class PageRequest
{
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 50;

    public PageRequest() { }

    public PageRequest(int page, int size)
    {
        Page = page < 1 ? 1 : page;
        Size = size < 1 ? 1 : (size > 500 ? 500 : size);
    }

    public static PageRequest Of(int page, int size) => new(page, size);

    public int Skip => (Page - 1) * Size;
    public int Take => Size;
}
```

---

## PageResponse<T>

```csharp
public class PageResponse<T>
{
    public List<T> Data { get; init; } = new();
    public bool Success { get; init; } = true;
 public long Total { get; init; }
    public int CurrentPage { get; init; }
    public int PageSize { get; init; }

    public PageResponse(List<T> data, bool success, long total, int currentPage, int pageSize)
    {
   Data = data ?? throw new ArgumentNullException(nameof(data));
Success = success;
 Total = total;
  CurrentPage = currentPage;
        PageSize = pageSize;
    }
}
```

---

## Usage

### Controller

```csharp
[HttpGet]
public async Task<ActionResult<PageResponse<Product>>> GetProducts(
    [FromQuery] int page = 1,
    [FromQuery] int size = 20,
    [FromQuery] string keyword = null)
{
 var result = await _productService.GetProducts(page, size, keyword);
  return Ok(result);
}
```

### Service

```csharp
public async Task<PageResponse<Product>> GetProducts(int page, int size, string keyword)
{
    var spec = new BaseSpecification<Product>()
        .WithCriteria(p => string.IsNullOrEmpty(keyword) || 
       p.NonUnicodeSearchString.Contains(keyword))
   .WithOrderBy(p => p.Name)
        .WithPagedResults(page, size);

    return await _productRepo.GetWithPagingAsync(spec);
}
```

---

## Response Example

```json
{
  "data": [
    { "id": 1, "name": "iPhone 15 Pro", "price": 29990000 },
    { "id": 2, "name": "Samsung Galaxy S24", "price": 24990000 }
  ],
  "success": true,
  "total": 100,
  "currentPage": 1,
  "pageSize": 20
}
```

---

## Frontend Integration

### React

```typescript
interface PageResponse<T> {
  data: T[];
  success: boolean;
  total: number;
  currentPage: number;
  pageSize: number;
}

const fetchProducts = async (page: number, size: number) => {
  const response = await fetch(`/api/products?page=${page}&size=${size}`);
  const result: PageResponse<Product> = await response.json();
  
  return {
    products: result.data,
    totalPages: Math.ceil(result.total / result.pageSize),
    currentPage: result.currentPage
  };
};
```

### Angular

```typescript
export interface PageResponse<T> {
data: T[];
  success: boolean;
  total: number;
  currentPage: number;
  pageSize: number;
}

getProducts(page: number, size: number): Observable<PageResponse<Product>> {
  return this.http.get<PageResponse<Product>>(
    `/api/products?page=${page}&size=${size}`
  );
}
```

---

**[? Back to Documentation](../README.md)**
