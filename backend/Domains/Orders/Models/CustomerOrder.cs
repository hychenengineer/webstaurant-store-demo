namespace Webstaurant.IDS.Api.Domains.Orders.Models;

public enum OrderStatus
{
    Pending,
    SentToVendor,
    Shipped,
    Delivered
}

public class CustomerOrder
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string ShipToAddress { get; set; } = string.Empty;
    public string ShipToCity { get; set; } = string.Empty;
    public string ShipToState { get; set; } = string.Empty;
    public string ShipToZip { get; set; } = string.Empty;
    public string OrderType { get; set; } = "Standard"; // Standard or DropShip
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? TrackingNumber { get; set; }
    public int? LinkedPoId { get; set; }

    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public int Id { get; set; }
    public int CustomerOrderId { get; set; }
    public int ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
