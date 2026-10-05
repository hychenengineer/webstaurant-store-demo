using MediatR;
using Webstaurant.IDS.Api.Domains.Inventory.Events;
using Webstaurant.IDS.Api.Domains.Inventory.Models;
using Webstaurant.IDS.Api.Domains.Purchasing.Models;
using Webstaurant.IDS.Api.Infrastructure.Repositories;

namespace Webstaurant.IDS.Api.Domains.Inventory.Services;

public interface IInventoryService
{
    Task<IEnumerable<StockBalance>> GetAllStockAsync();
    Task<IEnumerable<Product>> GetProductsAsync();
    Task<IEnumerable<Warehouse>> GetWarehousesAsync();
    Task DepleteStockAsync(int warehouseId, int productId, int quantity);
    Task AddInTransitStockAsync(int warehouseId, int productId, int quantity);
    Task ReceiveMerchandiseAsync(int purchaseOrderId);
}

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepo;
    private readonly IPurchasingRepository _poRepo;
    private readonly IMediator _mediator;

    public InventoryService(
        IInventoryRepository inventoryRepo,
        IPurchasingRepository poRepo,
        IMediator mediator)
    {
        _inventoryRepo = inventoryRepo;
        _poRepo = poRepo;
        _mediator = mediator;
    }

    public async Task<IEnumerable<StockBalance>> GetAllStockAsync()
    {
        return await _inventoryRepo.GetAllStockAsync();
    }

    public async Task<IEnumerable<Product>> GetProductsAsync()
    {
        return await _inventoryRepo.GetAllProductsAsync();
    }

    public async Task<IEnumerable<Warehouse>> GetWarehousesAsync()
    {
        return await _inventoryRepo.GetAllWarehousesAsync();
    }

    public async Task DepleteStockAsync(int warehouseId, int productId, int quantity)
    {
        var stock = await _inventoryRepo.GetStockAsync(warehouseId, productId);
        if (stock == null) throw new Exception("Stock record not found.");

        stock.QtyOnHand = Math.Max(0, stock.QtyOnHand - quantity);
        await _inventoryRepo.UpdateStockAsync(stock);

        // Check if under reorder point
        if (stock.QtyOnHand <= stock.Product.ReorderPoint)
        {
            await _mediator.Publish(new LowStockEvent(
                stock.ProductId,
                stock.Product.Sku,
                stock.WarehouseId,
                stock.QtyOnHand,
                stock.Product.ReorderPoint,
                stock.Product.TargetReplenishQty
            ));
        }
    }

    public async Task AddInTransitStockAsync(int warehouseId, int productId, int quantity)
    {
        var stock = await _inventoryRepo.GetStockAsync(warehouseId, productId);
        if (stock != null)
        {
            stock.QtyInTransit += quantity;
            await _inventoryRepo.UpdateStockAsync(stock);
        }
    }

    public async Task ReceiveMerchandiseAsync(int purchaseOrderId)
    {
        var po = await _poRepo.GetPoByIdAsync(purchaseOrderId);
        if (po == null) throw new Exception("Purchase Order not found.");
        if (po.WarehouseId == null) throw new Exception("PO has no assigned warehouse.");

        foreach (var item in po.Items)
        {
            var stock = await _inventoryRepo.GetStockAsync(po.WarehouseId.Value, item.ProductId);
            if (stock != null)
            {
                stock.QtyInTransit = Math.Max(0, stock.QtyInTransit - item.Quantity);
                stock.QtyOnHand += item.Quantity;
                await _inventoryRepo.UpdateStockAsync(stock);
            }
        }

        po.Status = PoStatus.Received;
        await _poRepo.UpdatePoAsync(po);
    }
}
