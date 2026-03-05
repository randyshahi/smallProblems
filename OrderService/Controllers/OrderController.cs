using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OrderService.Models;
using OrderService.Services;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService orderService;
        private readonly ILogger<OrderController> logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            this.orderService = orderService;
            this.logger = logger;
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders()
        {
            return Ok(await this.orderService.GetOrdersAsync());
        }

        [HttpPost("orders")]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            this.logger.LogInformation($"Creating order: {order.Id}");
            await this.orderService.CreateOrderAsync(order);
            return Ok("Order created");
        }

        [HttpPut("orders/{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            this.logger.LogInformation($"Cancelling order: {id}");
            bool isCancelled = await this.orderService.CancelOrderAsync(id);
            if(isCancelled)
            {
                return Ok();
            }
            return NotFound();
        }
    }
}
