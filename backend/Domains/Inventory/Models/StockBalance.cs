namespace Webstaurant.IDS.Api.Domains.Inventory.Models;

public class StockBalance
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public int QtyOnHand { get; set; }
    public int QtyReserved { get; set; }
    public int QtyInTransit { get; set; }

    public int AvailableToPromise => QtyOnHand - QtyReserved;
}
