namespace Webstaurant.IDS.Api.Domains.Inventory.Models;

public class Product
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public bool IsDropShipOnly { get; set; }
    public int ReorderPoint { get; set; }
    public int TargetReplenishQty { get; set; } = 25;
    public string VendorName { get; set; } = string.Empty;
}
