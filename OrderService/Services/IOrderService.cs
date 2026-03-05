using OrderService.Models;

namespace OrderService.Services
{
    public interface IOrderService
    {
        Task<List<Order>> GetOrdersAsync();
        Task CreateOrderAsync(Order order);
        Task<bool> CancelOrderAsync(int id);
    }
}