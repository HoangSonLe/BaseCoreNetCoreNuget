# ? Model Validation

## Automatic Validation

BaseNetCore.Core automatically validates DataAnnotations và returns standardized error responses.

---

## Setup

```csharp
// Program.cs - Already configured by AddBaseNetCoreFeatures()
builder.Services.AddAutomaticModelValidation();
```

---

## DataAnnotations

```csharp
public class CreateProductDto
{
    [Required(ErrorMessage = "Tên s?n ph?m là b?t bu?c")]
    [MaxLength(200, ErrorMessage = "Tên không ???c quá 200 ký t?")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Mã s?n ph?m là b?t bu?c")]
    [RegularExpression(@"^[A-Z]{2,4}\d{2,6}$", ErrorMessage = "Mã không h?p l?")]
    public string Code { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giá ph?i l?n h?n 0")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "S? l??ng không ???c âm")]
    public int Stock { get; set; }

    [EmailAddress(ErrorMessage = "Email không h?p l?")]
    public string ContactEmail { get; set; }
}
```

---

## Validation Response

**Request:**
```http
POST /api/products
{
  "name": "",
  "code": "invalid",
  "price": -100
}
```

**Response: 400**
```json
{
  "guid": "0HN7Q3QK3V8Q1:00000001",
  "code": "SYS004",
  "message": "D? li?u yêu c?u không h?p l?.",
  "path": "/api/products",
  "method": "POST",
  "timestamp": "2025-01-28T10:30:00.123Z",
  "errors": {
 "Name": ["Tên s?n ph?m là b?t bu?c"],
    "Code": ["Mã không h?p l?"],
    "Price": ["Giá ph?i l?n h?n 0"]
  }
}
```

---

## Custom Validation

```csharp
public class CreateProductDto : IValidatableObject
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public decimal DiscountPrice { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DiscountPrice >= Price)
        {
         yield return new ValidationResult(
                "Giá khuy?n mãi ph?i nh? h?n giá g?c",
   new[] { nameof(DiscountPrice) });
        }
    }
}
```

---

## FluentValidation (Optional)

```bash
dotnet add package FluentValidation.AspNetCore
```

```csharp
public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
 {
   RuleFor(x => x.Name)
      .NotEmpty().WithMessage("Tên là b?t bu?c")
  .MaximumLength(200).WithMessage("Tên không quá 200 ký t?");

    RuleFor(x => x.Price)
       .GreaterThan(0).WithMessage("Giá ph?i l?n h?n 0");

 RuleFor(x => x.DiscountPrice)
  .LessThan(x => x.Price).WithMessage("Giá KM ph?i nh? h?n giá g?c")
     .When(x => x.DiscountPrice > 0);
}
}

// Register
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>();
```

---

**[? Back to Documentation](../README.md)**
