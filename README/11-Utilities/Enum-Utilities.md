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
    [Description("?ang ch? x? lý")]
    Pending = 0,
    
    [Description("?ã xác nh?n")]
    Confirmed = 1,
    
[Description("?ang giao hàng")]
    Shipping = 2,
    
    [Description("?ã hoàn thành")]
    Completed = 3,
    
    [Description("?ã h?y")]
    Cancelled = 4
}

// Get description
var status = OrderStatus.Shipping;
var description = status.GetDescription();
// ? "?ang giao hàng"
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
//   "statusDisplay": "?ang giao hàng"
// }
```

---

**[? Back to Documentation](../README.md)**
