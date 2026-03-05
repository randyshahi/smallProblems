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

        /// <summary>
        /// Get all orders for a user
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="page">page number</param>
        /// <param name="pageSize">page size</param>
        /// <returns>paginated list of orders for the user</returns>
        [HttpGet("orders/{userId}")]
        public async Task<IActionResult> GetOrders(
            [FromRoute] string userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if(page <= 0)
            {
                return BadRequest("Page number must be greater than 0");
            }
            if(pageSize <= 0)
            {
                return BadRequest("Page size must be greater than 0");
            }
            if(String.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required");
            }
            if(pageSize > 100)
            {
                return BadRequest("Page size must be less than or equal to 100");
            }
            try
            {
                this.logger.LogInformation($"Getting orders for user: {userId} with page: {page} and pageSize: {pageSize}");
                List<Order> orders = await this.orderService.GetOrdersAsync(userId);
                return Ok(orders.Skip((page - 1) * pageSize).Take(pageSize));
            }
            catch(Exception ex)
            {
                this.logger.LogError(ex, $"Error getting orders for user: {userId}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("orders/{userId}")]
        public async Task<IActionResult> CreateOrder(
            [FromRoute] string userId,
            [FromBody] Order order)
        {
            if(String.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required");
            }
            if(order == null)
            {
                return BadRequest("Order is required");
            }
            this.logger.LogInformation($"Creating order: {order.Id} for user: {userId}");
            await this.orderService.CreateOrderAsync(userId, order);
            return Ok("Order created");
        }

        [HttpPut("orders/{userId}/cancel/{orderId}")]
        public async Task<IActionResult> CancelOrder(
            [FromRoute] string userId,
            [FromRoute] int orderId)
        {
            this.logger.LogInformation($"Cancelling order: {orderId} for user: {userId}");
            bool isCancelled = await this.orderService.CancelOrderAsync(userId, orderId);
            if(isCancelled)
            {
                return Ok();
            }
            return NotFound();
        }
    }
}
