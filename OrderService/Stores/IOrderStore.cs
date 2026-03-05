using OrderService.Models;

namespace OrderService.Stores
{
    public interface IOrderStore
    {
        Task<List<Order>> GetAllOrdersAsync(string userId);
        Task CreateOrderAsync(string userId, Order order);
        Task<bool> CancelOrderAsync(string userId, int orderId);
    }
}