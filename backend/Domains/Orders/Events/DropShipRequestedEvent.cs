using MediatR;

namespace Webstaurant.IDS.Api.Domains.Orders.Events;

public record DropShipRequestedEvent(
    int CustomerOrderId, 
    int ProductId, 
    string Sku, 
    int Quantity, 
    string VendorName, 
    string CustomerName, 
    string Address, 
    string City, 
    string State, 
    string Zip
) : INotification;
