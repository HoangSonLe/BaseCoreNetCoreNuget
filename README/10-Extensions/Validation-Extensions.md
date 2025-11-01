# ?? Validation Extensions

## AddAutomaticModelValidation

Enables automatic DataAnnotations validation.

```csharp
builder.Services.AddAutomaticModelValidation();
```

**Already included in:**
- `AddBaseNetCoreFeatures()`
- `AddBaseNetCoreFeaturesWithAuth()`

---

## How It Works

```csharp
// DTOs are automatically validated
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
{
    // If validation fails, returns 400 with ApiErrorResponse
    // including "errors" dictionary
}
```

---

## Validation Response

```json
{
  "guid": "...",
  "code": "SYS004",
"message": "D? li?u yêu c?u không h?p l?.",
  "errors": {
    "Name": ["Tên là b?t bu?c"],
    "Price": ["Giá ph?i l?n h?n 0"]
  }
}
```

---

## Disable Auto-Validation (per action)

```csharp
[HttpPost]
[ValidateNever]  // Custom attribute if needed
public async Task<IActionResult> CustomValidation([FromBody] ProductDto dto)
{
// Manual validation
 if (string.IsNullOrEmpty(dto.Name))
        throw new BadRequestException("Name is required");
}
```

---

**[? Back to Documentation](../README.md)**
