using Microsoft.EntityFrameworkCore;
using Webstaurant.IDS.Api.Domains.Inventory.Models;
using Webstaurant.IDS.Api.Infrastructure.Data;

namespace Webstaurant.IDS.Api.Infrastructure.Repositories;

public interface IInventoryRepository
{
    Task<IEnumerable<StockBalance>> GetAllStockAsync();
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<IEnumerable<Warehouse>> GetAllWarehousesAsync();
    Task<StockBalance?> GetStockAsync(int warehouseId, int productId);
    Task<StockBalance?> GetStockByIdAsync(int id);
    Task UpdateStockAsync(StockBalance stock);
}

public class InventoryRepository : IInventoryRepository
{
    private readonly AppDbContext _context;

    public InventoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StockBalance>> GetAllStockAsync()
    {
        return await _context.StockBalances
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task<IEnumerable<Warehouse>> GetAllWarehousesAsync()
    {
        return await _context.Warehouses.ToListAsync();
    }

    public async Task<StockBalance?> GetStockAsync(int warehouseId, int productId)
    {
        return await _context.StockBalances
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.WarehouseId == warehouseId && s.ProductId == productId);
    }

    public async Task<StockBalance?> GetStockByIdAsync(int id)
    {
        return await _context.StockBalances
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task UpdateStockAsync(StockBalance stock)
    {
        _context.StockBalances.Update(stock);
        await _context.SaveChangesAsync();
    }
}
