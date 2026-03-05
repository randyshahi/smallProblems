using OrderService.Models;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace OrderService.Stores
{
    public class OrderStore : IOrderStore
    {
        private readonly ILogger<OrderStore> logger;
        private ConcurrentDictionary<string, ConcurrentDictionary<int, Order>> inProgressOrders;
        private ConcurrentDictionary<string, ConcurrentDictionary<int, Order>> cancelledOrders;
        private ConcurrentDictionary<string, ConcurrentDictionary<int, Order>> processedOrders;
        private Channel<(string userId, Order order)> orderChannel;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        public OrderStore(ILogger<OrderStore> logger)
        {
            this.logger = logger;
            this.inProgressOrders = new ConcurrentDictionary<string, ConcurrentDictionary<int, Order>>();
            this.cancelledOrders = new ConcurrentDictionary<string, ConcurrentDictionary<int, Order>>();
            this.processedOrders = new ConcurrentDictionary<string, ConcurrentDictionary<int, Order>>();
            this.orderChannel = Channel.CreateBounded<(string userId, Order order)>(new BoundedChannelOptions(100) { FullMode = BoundedChannelFullMode.Wait });
            
            // spin up 5 different tasks to process orders
            for(int i = 0; i < 5; i++)
            {
                Task.Run(() => ProcessOrdersAsync(i));
            }
        }

        public async Task<List<Order>> GetAllOrdersAsync(string userId)
        {
            if(!this.inProgressOrders.ContainsKey(userId) && !this.cancelledOrders.ContainsKey(userId) && !this.processedOrders.ContainsKey(userId))
            {
                return new List<Order>();
            }
            return new List<Order>(
                this.inProgressOrders[userId].Values
                .Concat(this.cancelledOrders[userId].Values)
                .Concat(this.processedOrders[userId].Values)
                .ToList());
        }

        public async Task CreateOrderAsync(string userId, Order order)
        {
            this.inProgressOrders.AddOrUpdate(userId, new ConcurrentDictionary<int, Order>(), (key, existingOrder) => existingOrder);
            await this.orderChannel.Writer.WriteAsync((userId, order));
        }

        private async Task ProcessOrdersAsync(int index)
        {
            while(await this.orderChannel.Reader.WaitToReadAsync())
            {
                if(this.orderChannel.Reader.TryRead(out (string userId, Order order) orderTuple))
                {
                    // check if order has been cancelled
                    if(!this.cancelledOrders[orderTuple.userId].ContainsKey(orderTuple.order.Id))
                    {
                        this.logger.LogInformation($"Processing order: {orderTuple.order.Id} on core: {index}");
                        // order is in progress -> remove from in progress orders and add to processed orders
                        this.inProgressOrders[orderTuple.userId].TryRemove(orderTuple.order.Id, out _);
                        this.processedOrders[orderTuple.userId].TryAdd(orderTuple.order.Id, orderTuple.order);
                        this.logger.LogInformation($"Order: {orderTuple.order.Id} processed on core: {index}");
                    }
                    else
                    {
                        this.logger.LogInformation($"Order: {orderTuple.order.Id} cancelled on core: {index}");
                    }
                }
            }
        }

        public async Task<bool> CancelOrderAsync(string userId, int orderId)
        {
            if(this.cancelledOrders[userId].TryGetValue(orderId, out Order? order) && order != null)
            {
                this.cancelledOrders[userId].TryAdd(orderId, order);
                this.inProgressOrders[userId].TryRemove(orderId, out _);
                this.processedOrders[userId].TryAdd(orderId, order);
                return true;
            }
            return false;
        }
    }
}