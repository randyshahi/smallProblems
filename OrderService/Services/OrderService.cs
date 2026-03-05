using OrderService.Models;
using OrderService.Stores;

namespace OrderService.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderStore orderStore;

        public OrderService(IOrderStore orderStore)
        {
            this.orderStore = orderStore;
        }

        public async Task<List<Order>> GetOrdersAsync(string userId)
        {
            return await this.orderStore.GetAllOrdersAsync(userId);
        }

        public async Task CreateOrderAsync(string userId, Order order)
        {
            await this.orderStore.CreateOrderAsync(userId, order);
        }

        public async Task<bool> CancelOrderAsync(string userId, int orderId)
        {
            return await this.orderStore.CancelOrderAsync(userId, orderId);
        }
    }
}