using OrderService.Models;
using System.Threading.Channels;

namespace OrderService.Stores
{
    public class OrderStore : IOrderStore
    {
        private readonly object lockObject = new object();
        private Dictionary<int, Order> inProgressOrders;
        private Dictionary<int, Order> cancelledOrders;
        private Dictionary<int, Order> processedOrders;
        private Channel<Order> orderChannel;

        public OrderStore()
        {
            this.inProgressOrders = new Dictionary<int, Order>();
            this.cancelledOrders = new Dictionary<int, Order>();
            this.processedOrders = new Dictionary<int, Order>();
            this.orderChannel = Channel.CreateBounded<Order>(new BoundedChannelOptions(100) { FullMode = BoundedChannelFullMode.Wait });
            
            // spin up a task to process orders
            Task.Run(ProcessOrdersAsync);
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            List<Order> result = new List<Order>();
            result.AddRange(this.inProgressOrders.Values);
            result.AddRange(this.cancelledOrders.Values);
            result.AddRange(this.processedOrders.Values);
            return result;
        }

        public async Task CreateOrderAsync(Order order)
        {
            lock(this.lockObject)
            {
                this.inProgressOrders[order.Id] = order;
            }
            await this.orderChannel.Writer.WriteAsync(order);
        }

        private async Task ProcessOrdersAsync()
        {
            while(await this.orderChannel.Reader.WaitToReadAsync())
            {
                if(this.orderChannel.Reader.TryRead(out Order order))
                {
                    // check if order has been cancelled
                    lock(this.lockObject)
                    {  
                        if(!this.cancelledOrders.ContainsKey(order.Id))
                        {
                            // order is in progress -> remove from in progress orders and add to processed orders
                            this.inProgressOrders.Remove(order.Id);
                            this.processedOrders[order.Id] = order;
                        }
                    }
                }
            }
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            lock(this.lockObject)
            {
                if(this.inProgressOrders.ContainsKey(orderId))
                {
                    Order order = this.inProgressOrders[orderId];   
                    this.cancelledOrders.Add(orderId, order);
                    this.inProgressOrders.Remove(orderId);
                    return true;
                }
                return false;
            }   
        }
    }
}