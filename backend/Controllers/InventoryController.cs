using Microsoft.AspNetCore.Mvc;
using Webstaurant.IDS.Api.Domains.Inventory.Services;

namespace Webstaurant.IDS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllStock()
    {
        return Ok(await _inventoryService.GetAllStockAsync());
    }

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts()
    {
        return Ok(await _inventoryService.GetProductsAsync());
    }

    [HttpGet("warehouses")]
    public async Task<IActionResult> GetWarehouses()
    {
        return Ok(await _inventoryService.GetWarehousesAsync());
    }

    [HttpPost("deplete")]
    public async Task<IActionResult> DepleteStock([FromBody] DepleteStockRequest req)
    {
        await _inventoryService.DepleteStockAsync(req.WarehouseId, req.ProductId, req.Quantity);
        return Ok(new { message = "Stock depleted successfully" });
    }

    [HttpPost("receive")]
    public async Task<IActionResult> ReceiveMerchandise([FromBody] ReceiveMerchandiseRequest req)
    {
        await _inventoryService.ReceiveMerchandiseAsync(req.PurchaseOrderId);
        return Ok(new { message = "Merchandise received into inventory successfully" });
    }
}

public class DepleteStockRequest
{
    public int WarehouseId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class ReceiveMerchandiseRequest
{
    public int PurchaseOrderId { get; set; }
}
