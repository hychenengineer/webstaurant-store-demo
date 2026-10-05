using MediatR;
using Webstaurant.IDS.Api.Domains.Inventory.Events;
using Webstaurant.IDS.Api.Domains.Purchasing.Services;
using Webstaurant.IDS.Api.Infrastructure.Repositories;

namespace Webstaurant.IDS.Api.Domains.Purchasing.Handlers;

public class LowStockEventHandler : INotificationHandler<LowStockEvent>
{
    private readonly IPurchasingService _purchasingService;
    private readonly IPurchasingRepository _poRepo;

    public LowStockEventHandler(IPurchasingService purchasingService, IPurchasingRepository poRepo)
    {
        _purchasingService = purchasingService;
        _poRepo = poRepo;
    }

    public async Task Handle(LowStockEvent notification, CancellationToken cancellationToken)
    {
        // Guard against duplicate replenishment orders if one is already open
        bool hasOpen = await _poRepo.HasActivePoForProductAsync(notification.ProductId, notification.WarehouseId);
        if (hasOpen) return;

        // Auto-create replenishment PO and trigger EDI 850
        await _purchasingService.CreateReplenishmentPoAsync(
            notification.ProductId, 
            notification.WarehouseId, 
            notification.TargetReplenishQty
        );
    }
}
