# ?? String Utilities

## StringHelper Methods

```csharp
using BaseNetCore.Core.src.Main.Utils;

// Null/empty checks
StringHelper.IsNullOrEmpty(str);
StringHelper.IsNullOrWhiteSpace(str);
StringHelper.HasText(str);

// Trim
StringHelper.TrimToNull(str);  // Returns null if empty after trim
StringHelper.TrimToEmpty(str);  // Returns "" if null

// Case
StringHelper.EqualsIgnoreCase(str1, str2);
StringHelper.Capitalize(str);  // "hello" -> "Hello"
StringHelper.Uncapitalize(str);  // "Hello" -> "hello"

// Truncate
StringHelper.Truncate(str, maxLength);

// Contains
StringHelper.Contains(source, toCheck);  // Case-sensitive
StringHelper.ContainsIgnoreCase(source, toCheck);
```

---

## Examples

```csharp
// Validation
if (!StringHelper.HasText(name))
    throw new BadRequestException("Name is required");

// Case-insensitive comparison
if (StringHelper.EqualsIgnoreCase(role, "Admin"))
{
    // ...
}

// Truncate for display
var shortDesc = StringHelper.Truncate(description, 100);
```

---

**[? Back to Documentation](../README.md)**
