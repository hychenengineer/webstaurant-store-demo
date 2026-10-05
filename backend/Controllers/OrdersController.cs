using Microsoft.AspNetCore.Mvc;
using Webstaurant.IDS.Api.Domains.Orders.Services;

namespace Webstaurant.IDS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllOrders()
    {
        return Ok(await _orderService.GetAllOrdersAsync());
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] CreateOrderDto dto)
    {
        var order = await _orderService.PlaceOrderAsync(dto);
        return Ok(order);
    }
}
