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

        public async Task<List<Order>> GetOrdersAsync()
        {
            return await this.orderStore.GetAllOrdersAsync();

            // can do some filtering here if needed
        }

        public async Task CreateOrderAsync(Order order)
        {
            await this.orderStore.CreateOrderAsync(order);
        }

        public async Task<bool> CancelOrderAsync(int id)
        {
            return await this.orderStore.CancelOrderAsync(id);
        }
    }
}