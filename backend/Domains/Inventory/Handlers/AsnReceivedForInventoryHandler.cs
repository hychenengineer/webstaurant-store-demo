using MediatR;
using Webstaurant.IDS.Api.Domains.EDI.Events;
using Webstaurant.IDS.Api.Domains.Inventory.Services;
using Webstaurant.IDS.Api.Infrastructure.Repositories;

namespace Webstaurant.IDS.Api.Domains.Inventory.Handlers;

public class AsnReceivedForInventoryHandler : INotificationHandler<AsnReceivedEvent>
{
    private readonly IInventoryService _inventoryService;
    private readonly IPurchasingRepository _poRepo;

    public AsnReceivedForInventoryHandler(IInventoryService inventoryService, IPurchasingRepository poRepo)
    {
        _inventoryService = inventoryService;
        _poRepo = poRepo;
    }

    public async Task Handle(AsnReceivedEvent notification, CancellationToken cancellationToken)
    {
        var po = await _poRepo.GetPoByIdAsync(notification.PurchaseOrderId);
        if (po != null && po.PoType == "Replenishment" && po.WarehouseId.HasValue)
        {
            foreach (var item in po.Items)
            {
                await _inventoryService.AddInTransitStockAsync(po.WarehouseId.Value, item.ProductId, item.Quantity);
            }
        }
    }
}
