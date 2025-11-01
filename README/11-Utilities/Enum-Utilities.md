# ?? Enum Utilities

## GetDescription

Gets description attribute c?a enum value.

---

## Usage

```csharp
using BaseNetCore.Core.src.Main.Utils;
using System.ComponentModel;

public enum OrderStatus
{
    [Description("?ang ch? x? l�")]
    Pending = 0,
    
    [Description("?� x�c nh?n")]
    Confirmed = 1,
    
[Description("?ang giao h�ng")]
    Shipping = 2,
    
    [Description("?� ho�n th�nh")]
    Completed = 3,
    
    [Description("?� h?y")]
    Cancelled = 4
}

// Get description
var status = OrderStatus.Shipping;
var description = status.GetDescription();
// ? "?ang giao h�ng"
```

---

## Without Description Attribute

```csharp
public enum PaymentMethod
{
Cash,
    CreditCard,
    BankTransfer
}

var method = PaymentMethod.CreditCard;
var description = method.GetDescription();
// ? "CreditCard" (enum name)
```

---

## Display in API

```csharp
public class OrderDto
{
    public int Id { get; set; }
    public OrderStatus Status { get; set; }
  public string StatusDisplay => Status.GetDescription();
}

// Response:
// {
//   "id": 1,
//   "status": 2,
//   "statusDisplay": "?ang giao h�ng"
// }
```

---

**[? Back to Documentation](../README.md)**
