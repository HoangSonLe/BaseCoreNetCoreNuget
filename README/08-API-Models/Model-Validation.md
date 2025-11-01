# ? Model Validation

## Automatic Validation

BaseNetCore.Core automatically validates DataAnnotations v� returns standardized error responses.

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
    [Required(ErrorMessage = "T�n s?n ph?m l� b?t bu?c")]
    [MaxLength(200, ErrorMessage = "T�n kh�ng ???c qu� 200 k� t?")]
    public string Name { get; set; }

    [Required(ErrorMessage = "M� s?n ph?m l� b?t bu?c")]
    [RegularExpression(@"^[A-Z]{2,4}\d{2,6}$", ErrorMessage = "M� kh�ng h?p l?")]
    public string Code { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Gi� ph?i l?n h?n 0")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "S? l??ng kh�ng ???c �m")]
    public int Stock { get; set; }

    [EmailAddress(ErrorMessage = "Email kh�ng h?p l?")]
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
  "message": "D? li?u y�u c?u kh�ng h?p l?.",
  "path": "/api/products",
  "method": "POST",
  "timestamp": "2025-01-28T10:30:00.123Z",
  "errors": {
 "Name": ["T�n s?n ph?m l� b?t bu?c"],
    "Code": ["M� kh�ng h?p l?"],
    "Price": ["Gi� ph?i l?n h?n 0"]
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
                "Gi� khuy?n m�i ph?i nh? h?n gi� g?c",
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
      .NotEmpty().WithMessage("T�n l� b?t bu?c")
  .MaximumLength(200).WithMessage("T�n kh�ng qu� 200 k� t?");

    RuleFor(x => x.Price)
       .GreaterThan(0).WithMessage("Gi� ph?i l?n h?n 0");

 RuleFor(x => x.DiscountPrice)
  .LessThan(x => x.Price).WithMessage("Gi� KM ph?i nh? h?n gi� g?c")
     .When(x => x.DiscountPrice > 0);
}
}

// Register
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>();
```

---

**[? Back to Documentation](../README.md)**
