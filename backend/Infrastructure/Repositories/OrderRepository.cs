using Microsoft.EntityFrameworkCore;
using Webstaurant.IDS.Api.Domains.Orders.Models;
using Webstaurant.IDS.Api.Infrastructure.Data;

namespace Webstaurant.IDS.Api.Infrastructure.Repositories;

public interface IOrderRepository
{
    Task<IEnumerable<CustomerOrder>> GetAllOrdersAsync();
    Task<CustomerOrder?> GetOrderByIdAsync(int id);
    Task<CustomerOrder> CreateOrderAsync(CustomerOrder order);
    Task UpdateOrderAsync(CustomerOrder order);
}

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CustomerOrder>> GetAllOrdersAsync()
    {
        return await _context.CustomerOrders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<CustomerOrder?> GetOrderByIdAsync(int id)
    {
        return await _context.CustomerOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<CustomerOrder> CreateOrderAsync(CustomerOrder order)
    {
        _context.CustomerOrders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task UpdateOrderAsync(CustomerOrder order)
    {
        _context.CustomerOrders.Update(order);
        await _context.SaveChangesAsync();
    }
}
