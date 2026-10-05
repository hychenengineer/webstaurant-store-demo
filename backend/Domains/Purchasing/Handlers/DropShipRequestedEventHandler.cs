using MediatR;
using Webstaurant.IDS.Api.Domains.Orders.Events;
using Webstaurant.IDS.Api.Domains.Purchasing.Services;

namespace Webstaurant.IDS.Api.Domains.Purchasing.Handlers;

public class DropShipRequestedEventHandler : INotificationHandler<DropShipRequestedEvent>
{
    private readonly IPurchasingService _purchasingService;

    public DropShipRequestedEventHandler(IPurchasingService purchasingService)
    {
        _purchasingService = purchasingService;
    }

    public async Task Handle(DropShipRequestedEvent notification, CancellationToken cancellationToken)
    {
        // Auto-create Drop-Ship PO with customer address and generate EDI 850
        await _purchasingService.CreateDropShipPoAsync(notification);
    }
}
