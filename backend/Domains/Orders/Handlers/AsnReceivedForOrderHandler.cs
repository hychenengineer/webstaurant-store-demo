using MediatR;
using Webstaurant.IDS.Api.Domains.EDI.Events;
using Webstaurant.IDS.Api.Domains.Orders.Services;
using Webstaurant.IDS.Api.Infrastructure.Repositories;

namespace Webstaurant.IDS.Api.Domains.Orders.Handlers;

public class AsnReceivedForOrderHandler : INotificationHandler<AsnReceivedEvent>
{
    private readonly IOrderService _orderService;
    private readonly IPurchasingRepository _poRepo;

    public AsnReceivedForOrderHandler(IOrderService orderService, IPurchasingRepository poRepo)
    {
        _orderService = orderService;
        _poRepo = poRepo;
    }

    public async Task Handle(AsnReceivedEvent notification, CancellationToken cancellationToken)
    {
        var po = await _poRepo.GetPoByIdAsync(notification.PurchaseOrderId);
        if (po != null && po.PoType == "DropShip" && po.CustomerOrderId.HasValue)
        {
            await _orderService.MarkOrderShippedAsync(po.CustomerOrderId.Value, notification.TrackingNumber);
        }
    }
}
