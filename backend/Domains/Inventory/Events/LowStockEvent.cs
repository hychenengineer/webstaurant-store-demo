using MediatR;

namespace Webstaurant.IDS.Api.Domains.Inventory.Events;

public record LowStockEvent(
    int ProductId, 
    string Sku, 
    int WarehouseId, 
    int CurrentQtyOnHand, 
    int ReorderPoint, 
    int TargetReplenishQty
) : INotification;
