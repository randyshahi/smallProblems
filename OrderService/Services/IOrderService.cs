using OrderService.Models;

namespace OrderService.Services
{
    public interface IOrderService
    {
        /// <summary>
        /// Get all orders for a user
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>list of orders for the user</returns>
        Task<List<Order>> GetOrdersAsync(string userId);

        /// <summary>
        /// Create an order for a user
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="order">order to create</param>
        /// <returns>true if order was created, false otherwise</returns>
        Task CreateOrderAsync(string userId, Order order);

        /// <summary>
        /// Cancel an order for a user
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="orderId">order id to cancel</param>
        /// <returns>true if order was cancelled, false otherwise</returns>
        Task<bool> CancelOrderAsync(string userId, int orderId);
    }
}