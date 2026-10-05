using MediatR;
using Webstaurant.IDS.Api.Domains.Inventory.Services;
using Webstaurant.IDS.Api.Domains.Orders.Events;
using Webstaurant.IDS.Api.Domains.Orders.Models;
using Webstaurant.IDS.Api.Infrastructure.Repositories;

namespace Webstaurant.IDS.Api.Domains.Orders.Services;

public interface IOrderService
{
    Task<IEnumerable<CustomerOrder>> GetAllOrdersAsync();
    Task<CustomerOrder> PlaceOrderAsync(CreateOrderDto dto);
    Task MarkOrderShippedAsync(int orderId, string trackingNumber);
}

public class CreateOrderDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    public int? WarehouseId { get; set; }
}

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepo;
    private readonly IInventoryRepository _inventoryRepo;
    private readonly IInventoryService _inventoryService;
    private readonly IMediator _mediator;

    public OrderService(
        IOrderRepository orderRepo,
        IInventoryRepository inventoryRepo,
        IInventoryService inventoryService,
        IMediator mediator)
    {
        _orderRepo = orderRepo;
        _inventoryRepo = inventoryRepo;
        _inventoryService = inventoryService;
        _mediator = mediator;
    }

    public async Task<IEnumerable<CustomerOrder>> GetAllOrdersAsync()
    {
        return await _orderRepo.GetAllOrdersAsync();
    }

    public async Task<CustomerOrder> PlaceOrderAsync(CreateOrderDto dto)
    {
        var product = (await _inventoryRepo.GetAllProductsAsync()).FirstOrDefault(p => p.Id == dto.ProductId)
            ?? throw new Exception("Product not found.");

        var order = new CustomerOrder
        {
            OrderNumber = $"SO-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
            CustomerName = dto.CustomerName,
            ShipToAddress = dto.Address,
            ShipToCity = dto.City,
            ShipToState = dto.State,
            ShipToZip = dto.Zip,
            CreatedAt = DateTime.UtcNow,
            OrderType = product.IsDropShipOnly ? "DropShip" : "Standard",
            Status = OrderStatus.Pending,
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductId = product.Id,
                    Sku = product.Sku,
                    ProductName = product.Name,
                    Quantity = dto.Quantity,
                    UnitPrice = product.UnitPrice
                }
            }
        };

        await _orderRepo.CreateOrderAsync(order);

        if (product.IsDropShipOnly)
        {
            // Drop-Ship workflow: publish event for Purchasing/EDI
            await _mediator.Publish(new DropShipRequestedEvent(
                order.Id,
                product.Id,
                product.Sku,
                dto.Quantity,
                product.VendorName,
                order.CustomerName,
                order.ShipToAddress,
                order.ShipToCity,
                order.ShipToState,
                order.ShipToZip
            ));

            order.Status = OrderStatus.SentToVendor;
            await _orderRepo.UpdateOrderAsync(order);
        }
        else
        {
            // Regular fulfillment: deplete stock from chosen DC or default Lititz
            int whId = dto.WarehouseId ?? 1;
            await _inventoryService.DepleteStockAsync(whId, product.Id, dto.Quantity);
            order.Status = OrderStatus.Shipped;
            order.TrackingNumber = "1Z" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16).ToUpper();
            await _orderRepo.UpdateOrderAsync(order);
        }

        return order;
    }

    public async Task MarkOrderShippedAsync(int orderId, string trackingNumber)
    {
        var order = await _orderRepo.GetOrderByIdAsync(orderId);
        if (order != null)
        {
            order.Status = OrderStatus.Shipped;
            order.TrackingNumber = trackingNumber;
            await _orderRepo.UpdateOrderAsync(order);
        }
    }
}
