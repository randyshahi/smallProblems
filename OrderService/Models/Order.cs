namespace OrderService.Models
{
    public class Order
    {
        public Order(int id, string productName, int quantity, decimal price)
        {
            this.Id = id;
            this.ProductName = productName;
            this.Quantity = quantity;
            this.Price = price;
        }
        public int Id { get; private set; }
        public string ProductName { get; private set; }
        public int Quantity { get; private set; }
        public decimal Price { get; private set; }
    }
}