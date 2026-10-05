using Microsoft.EntityFrameworkCore;
using Webstaurant.IDS.Api.Domains.Purchasing.Models;
using Webstaurant.IDS.Api.Infrastructure.Data;

namespace Webstaurant.IDS.Api.Infrastructure.Repositories;

public interface IPurchasingRepository
{
    Task<IEnumerable<VendorPurchaseOrder>> GetAllPosAsync();
    Task<VendorPurchaseOrder?> GetPoByIdAsync(int id);
    Task<VendorPurchaseOrder?> GetPoByNumberAsync(string poNumber);
    Task<VendorPurchaseOrder> CreatePoAsync(VendorPurchaseOrder po);
    Task UpdatePoAsync(VendorPurchaseOrder po);
    Task<bool> HasActivePoForProductAsync(int productId, int? warehouseId);
}

public class PurchasingRepository : IPurchasingRepository
{
    private readonly AppDbContext _context;

    public PurchasingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VendorPurchaseOrder>> GetAllPosAsync()
    {
        return await _context.PurchaseOrders
            .Include(p => p.Items)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<VendorPurchaseOrder?> GetPoByIdAsync(int id)
    {
        return await _context.PurchaseOrders
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<VendorPurchaseOrder?> GetPoByNumberAsync(string poNumber)
    {
        return await _context.PurchaseOrders
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.PoNumber == poNumber);
    }

    public async Task<VendorPurchaseOrder> CreatePoAsync(VendorPurchaseOrder po)
    {
        _context.PurchaseOrders.Add(po);
        await _context.SaveChangesAsync();
        return po;
    }

    public async Task UpdatePoAsync(VendorPurchaseOrder po)
    {
        _context.PurchaseOrders.Update(po);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasActivePoForProductAsync(int productId, int? warehouseId)
    {
        return await _context.PurchaseOrders
            .Where(p => p.WarehouseId == warehouseId && (p.Status == PoStatus.Pending || p.Status == PoStatus.Sent850 || p.Status == PoStatus.InTransit856))
            .AnyAsync(p => p.Items.Any(i => i.ProductId == productId));
    }
}
