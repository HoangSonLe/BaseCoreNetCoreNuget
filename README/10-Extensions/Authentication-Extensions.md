# ?? Authentication Extensions

## AddJwtAuthentication

Configures JWT authentication with RSA validation.

```csharp
builder.Services.AddJwtAuthentication(builder.Configuration);
```

**Configuration required:**
```json
{
  "TokenSettings": {
    "RsaPrivateKey": "-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----",
    "RsaPublicKey": "-----BEGIN PUBLIC KEY-----\n...\n-----END PUBLIC KEY-----",
    "AccessExpireTimeS": "3600",
    "RefreshExpireTimeS": "86400",
 "Issuer": "MyApp",
    "Audience": "MyAppUsers"
  }
}
```

---

## AddTokenService

Registers `ITokenService` implementation.

```csharp
builder.Services.AddTokenService(builder.Configuration);
```

---

## Custom Token Validator

Implement `ITokenValidator` for database checks:

```csharp
public class DatabaseTokenValidator : ITokenValidator
{
    public async Task<bool> ValidateAsync(ClaimsPrincipal principal, string rawToken, HttpContext httpContext)
    {
   // Check user exists and is active
        // Check token not blacklisted
   return true;
    }
}

// Auto-registered by AddBaseNetCoreFeaturesWithAuth()
```

---

**[? Back to Documentation](../README.md)**
