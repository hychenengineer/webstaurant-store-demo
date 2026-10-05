namespace Webstaurant.IDS.Api.Domains.Purchasing.Models;

public enum PoStatus
{
    Pending,
    Sent850,
    InTransit856,
    Received
}

public class VendorPurchaseOrder
{
    public int Id { get; set; }
    public string PoNumber { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public string PoType { get; set; } = "Replenishment"; // Replenishment or DropShip
    public int? WarehouseId { get; set; }
    public int? CustomerOrderId { get; set; }
    public PoStatus Status { get; set; } = PoStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ShippedAt { get; set; }
    public string? TrackingNumber { get; set; }
    public string? Raw850Payload { get; set; }
    public string? Raw856Payload { get; set; }

    public List<PurchaseOrderItem> Items { get; set; } = new();
}

public class PurchaseOrderItem
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public int ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
