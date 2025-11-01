# ?? Searchable Fields

## Giới thiệu

**Searchable Fields** cho phép tìm ki?m ti?ng Vi?t **không d?u** với **opt-in design**.

---

## Attributes

### [SearchableEntity]

?ánh d?u entity **c?n** search support:

```csharp
[SearchableEntity]
public class Product : BaseSearchableEntity
{
  public int Id { get; set; }
    
    [SearchableField(Order = 1)]
    public string Name { get; set; }
}
```

### [SearchableField]

?ánh d?u property là **searchable**:

```csharp
[SearchableField(Order = 1)]  // Appears first in search string
public string Name { get; set; }

[SearchableField(Name = "ProductCode", Order = 2)]
public string Code { get; set; }
```

---

## Implementation Approaches

### Approach 1: Inherit BaseSearchableEntity (Recommended)

```csharp
[SearchableEntity]
public class Product : BaseSearchableEntity
{
    [SearchableField(Order = 1)]
    public string Name { get; set; }
    
    [SearchableField(Order = 2)]
    public string Code { get; set; }

    // NonUnicodeSearchString auto-generated
}
```

### Approach 2: Implement ISearchableEntity

```csharp
[SearchableEntity]
public class Customer : BaseAuditableEntity, ISearchableEntity
{
    [SearchableField(Order = 1)]
    public string FullName { get; set; }
    
    public string NonUnicodeSearchString { get; set; }
    
    public void GenerateSearchString()
    {
        NonUnicodeSearchString = SearchFieldUtils.BuildString(this);
    }
}
```

### Approach 3: Custom Generation

```csharp
[SearchableEntity]
public class Employee : BaseSearchableEntity
{
    [SearchableField(Order = 1)]
    public string FullName { get; set; }
    
    public string Email { get; set; }
    
    public override void GenerateSearchString()
    {
        base.GenerateSearchString();
        
    // Add custom logic
   if (!string.IsNullOrEmpty(Email))
        {
            var emailNormalized = SearchFieldUtils.RemoveVietnameseDiacritics(Email);
    NonUnicodeSearchString = $"{NonUnicodeSearchString} {emailNormalized}".Trim();
      }
    }
}
```

---

## How It Works

```csharp
var product = new Product
{
    Name = "?i?n tho?i iPhone 15 Pro",
    Code = "IP15-PRO-256"
};

// Auto-generated when saved to database:
// NonUnicodeSearchString = "dien thoai iphone 15 pro ip15-pro-256"
```

### Search Examples

```csharp
// All these work:
var products1 = await SearchProducts("iphone");  // ?
var products2 = await SearchProducts("dien thoai");  // ? (no diacritics)
var products3 = await SearchProducts("ip15");  // ?
var products4 = await SearchProducts("?i?n tho?i");  // ? (with diacritics)
```

---

## Search Service

```csharp
public async Task<List<Product>> SearchProducts(string keyword)
{
    var normalized = SearchFieldUtils.NormalizeSearchText(keyword);
    
    return await _productRepo.GetAllAsync(
     filter: p => p.NonUnicodeSearchString.Contains(normalized),
        tracking: false
    );
}
```

---

## Opt-in Design Benefits

### ? Entities WITH [SearchableEntity]

```csharp
[SearchableEntity]
public class Product : BaseSearchableEntity
{
    // ? NonUnicodeSearchString generated
    // ? Searchable
    // ?? Slightly slower inserts
}
```

### ? Entities WITHOUT [SearchableEntity]

```csharp
// ? No [SearchableEntity] attribute
public class Order : BaseAuditableEntity
{
    // ? No NonUnicodeSearchString
    // ? Not searchable
    // ? Faster insert/update
}
```

---

## Performance

| Operation | With Search | Without Search |
|-----------|-------------|----------------|
| **Insert** | ?? +5-10% overhead | ? Baseline |
| **Update** | ?? +5-10% overhead | ? Baseline |
| **Search** | ? Fast (indexed) | ? N/A |

**Recommendation:** Only use for entities that **need full-text search**.

---

**[? Back to Documentation](../README.md)**
