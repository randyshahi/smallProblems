using OrderService.Models;

namespace OrderService.Stores
{
    public interface IOrderStore
    {
        Task<List<Order>> GetAllOrdersAsync();
        Task CreateOrderAsync(Order order);
        Task<bool> CancelOrderAsync(int id);
    }
}