# ?? API Responses

## ApiErrorResponse

**Used by GlobalExceptionMiddleware**

```csharp
public class ApiErrorResponse
{
    public string Guid { get; set; }  // Request trace ID
    public string Code { get; set; }  // Error code
    public string Message { get; set; }  // Error message
public string Path { get; set; }  // Request path
    public string Method { get; set; }  // HTTP method
    public DateTime Timestamp { get; set; }  // UTC timestamp
    public Dictionary<string, string[]>? Errors { get; set; }  // Validation errors
}
```

### Example

```json
{
  "guid": "0HN7Q3QK3V8Q1:00000001",
  "code": "SYS005",
  "message": "Product not found",
  "path": "/api/products/999",
  "method": "GET",
  "timestamp": "2025-01-28T10:30:00.123Z"
}
```

---

## PageResponse<T>

**Used for paginated lists**

```csharp
public class PageResponse<T>
{
  public List<T> Data { get; init; }
    public bool Success { get; init; } = true;
    public long Total { get; init; }
    public int CurrentPage { get; init; }
    public int PageSize { get; init; }
}
```

### Example

```json
{
  "data": [...],
  "success": true,
  "total": 100,
  "currentPage": 1,
  "pageSize": 20
}
```

---

## ValueResponse<T>

**Used for single value responses**

```csharp
public record ValueResponse<T>(T Value);
```

### Example

```json
{
  "value": 12345
}
```

---

## Best Practices

### ? Consistent Structure

```csharp
// Success with data
return Ok(product);

// Success with message
return Ok(new { message = "Product created successfully" });

// Error (handled by middleware)
throw new ResourceNotFoundException("Product not found");
```

### ? HTTP Status Codes

| Status | Usage |
|--------|-------|
| 200 OK | Success with data |
| 201 Created | Resource created |
| 204 No Content | Success without data |
| 400 Bad Request | Invalid input |
| 401 Unauthorized | Not authenticated |
| 403 Forbidden | No permission |
| 404 Not Found | Resource not found |
| 409 Conflict | Data conflict |
| 500 Internal Server Error | Server error |

---

**[? Back to Documentation](../README.md)**
