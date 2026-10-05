using MediatR;

namespace Webstaurant.IDS.Api.Domains.EDI.Events;

public record AsnReceivedEvent(
    int PurchaseOrderId, 
    string PoNumber, 
    string TrackingNumber, 
    DateTime ShipDate, 
    string RawPayload
) : INotification;
