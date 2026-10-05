using MediatR;
using Webstaurant.IDS.Api.Domains.EDI.Events;
using Webstaurant.IDS.Api.Domains.EDI.Models;
using Webstaurant.IDS.Api.Domains.EDI.Services;
using Webstaurant.IDS.Api.Domains.Inventory.Models;
using Webstaurant.IDS.Api.Domains.Orders.Events;
using Webstaurant.IDS.Api.Domains.Purchasing.Models;
using Webstaurant.IDS.Api.Infrastructure.Repositories;

namespace Webstaurant.IDS.Api.Domains.Purchasing.Services;

public interface IPurchasingService
{
    Task<IEnumerable<VendorPurchaseOrder>> GetAllPosAsync();
    Task<VendorPurchaseOrder?> GetPoByIdAsync(int id);
    Task<VendorPurchaseOrder> CreateReplenishmentPoAsync(int productId, int warehouseId, int quantity);
    Task<VendorPurchaseOrder> CreateDropShipPoAsync(DropShipRequestedEvent dropShipReq);
    Task<VendorPurchaseOrder> ProcessVendorAsnAsync(string rawX12);
    Task MarkPoReceivedAsync(int poId);
}

public class PurchasingService : IPurchasingService
{
    private readonly IPurchasingRepository _poRepo;
    private readonly IInventoryRepository _inventoryRepo;
    private readonly IEdiService _ediService;
    private readonly IEdiRepository _ediRepo;
    private readonly IMediator _mediator;

    public PurchasingService(
        IPurchasingRepository poRepo,
        IInventoryRepository inventoryRepo,
        IEdiService ediService,
        IEdiRepository ediRepo,
        IMediator mediator)
    {
        _poRepo = poRepo;
        _inventoryRepo = inventoryRepo;
        _ediService = ediService;
        _ediRepo = ediRepo;
        _mediator = mediator;
    }

    public async Task<IEnumerable<VendorPurchaseOrder>> GetAllPosAsync()
    {
        return await _poRepo.GetAllPosAsync();
    }

    public async Task<VendorPurchaseOrder?> GetPoByIdAsync(int id)
    {
        return await _poRepo.GetPoByIdAsync(id);
    }

    public async Task<VendorPurchaseOrder> CreateReplenishmentPoAsync(int productId, int warehouseId, int quantity)
    {
        var product = (await _inventoryRepo.GetAllProductsAsync()).FirstOrDefault(p => p.Id == productId)
            ?? throw new ArgumentException("Product not found");

        var po = new VendorPurchaseOrder
        {
            PoNumber = $"PO-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
            VendorName = product.VendorName,
            PoType = "Replenishment",
            WarehouseId = warehouseId,
            Status = PoStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Items = new List<PurchaseOrderItem>
            {
                new PurchaseOrderItem
                {
                    ProductId = product.Id,
                    Sku = product.Sku,
                    ProductName = product.Name,
                    Quantity = quantity,
                    UnitPrice = product.UnitPrice
                }
            }
        };

        await _poRepo.CreatePoAsync(po);

        // Generate EDI 850
        var raw850 = await _ediService.Generate850PurchaseOrderAsync(po);
        po.Status = PoStatus.Sent850;
        po.Raw850Payload = raw850;
        await _poRepo.UpdatePoAsync(po);

        return po;
    }

    public async Task<VendorPurchaseOrder> CreateDropShipPoAsync(DropShipRequestedEvent dropShipReq)
    {
        var product = (await _inventoryRepo.GetAllProductsAsync()).FirstOrDefault(p => p.Id == dropShipReq.ProductId)
            ?? throw new ArgumentException("Product not found");

        var po = new VendorPurchaseOrder
        {
            PoNumber = $"PO-DS-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
            VendorName = dropShipReq.VendorName,
            PoType = "DropShip",
            CustomerOrderId = dropShipReq.CustomerOrderId,
            Status = PoStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Items = new List<PurchaseOrderItem>
            {
                new PurchaseOrderItem
                {
                    ProductId = product.Id,
                    Sku = product.Sku,
                    ProductName = product.Name,
                    Quantity = dropShipReq.Quantity,
                    UnitPrice = product.UnitPrice
                }
            }
        };

        await _poRepo.CreatePoAsync(po);

        // Generate EDI 850 with Customer address in Ship-To (N1*ST)
        var raw850 = await _ediService.Generate850PurchaseOrderAsync(
            po, 
            shipToName: dropShipReq.CustomerName, 
            shipToAddress: dropShipReq.Address, 
            shipToCity: dropShipReq.City, 
            shipToState: dropShipReq.State, 
            shipToZip: dropShipReq.Zip);

        po.Status = PoStatus.Sent850;
        po.Raw850Payload = raw850;
        await _poRepo.UpdatePoAsync(po);

        return po;
    }

    public async Task<VendorPurchaseOrder> ProcessVendorAsnAsync(string rawX12)
    {
        var (poNumber, trackingNumber, shipDate) = _ediService.Parse856AdvanceShipNotice(rawX12);
        var po = await _poRepo.GetPoByNumberAsync(poNumber);
        if (po == null)
        {
            // If PO number wasn't exact match, match the latest Sent850 PO
            po = (await _poRepo.GetAllPosAsync()).FirstOrDefault(p => p.Status == PoStatus.Sent850)
                ?? throw new Exception("No pending PO found to attach ASN.");
        }

        po.Status = PoStatus.InTransit856;
        po.TrackingNumber = trackingNumber;
        po.ShippedAt = shipDate;
        po.Raw856Payload = rawX12;
        await _poRepo.UpdatePoAsync(po);

        // Record inbound EDI 856 transaction
        await _ediRepo.SaveTransactionAsync(new EdiTransaction
        {
            TransactionSet = "856",
            ControlNumber = $"{po.Id:D9}",
            Direction = "Inbound",
            PartnerName = po.VendorName,
            PoNumber = po.PoNumber,
            RawPayload = rawX12,
            CreatedAt = DateTime.UtcNow
        });

        // Publish event for Inventory or Orders
        await _mediator.Publish(new AsnReceivedEvent(po.Id, po.PoNumber, trackingNumber, shipDate, rawX12));

        return po;
    }

    public async Task MarkPoReceivedAsync(int poId)
    {
        var po = await _poRepo.GetPoByIdAsync(poId);
        if (po != null)
        {
            po.Status = PoStatus.Received;
            await _poRepo.UpdatePoAsync(po);
        }
    }
}
