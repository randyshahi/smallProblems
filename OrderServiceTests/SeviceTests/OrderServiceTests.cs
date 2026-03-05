using Moq;
using OrderService.Models;
using OrderService.Services;
using OrderService.Stores;

namespace OrderServiceTests.SeviceTests;

public class OrderServiceTests
{
    private readonly Mock<IOrderStore> _orderStoreMock;
    private readonly OrderService.Services.OrderService _sut;

    public OrderServiceTests()
    {
        _orderStoreMock = new Mock<IOrderStore>();
        _sut = new OrderService.Services.OrderService(_orderStoreMock.Object);
    }

    [Fact]
    public async Task GetOrdersAsync_ReturnsAllOrdersFromStore()
    {
        var expectedOrders = new List<Order>
        {
            new Order(1, "Widget", 2, 9.99m),
            new Order(2, "Gadget", 1, 19.99m)
        };
        _orderStoreMock
            .Setup(s => s.GetAllOrdersAsync())
            .ReturnsAsync(expectedOrders);

        var result = await _sut.GetOrdersAsync();

        Assert.Same(expectedOrders, result);
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("Widget", result[0].ProductName);
        Assert.Equal(2, result[0].Quantity);
        Assert.Equal(9.99m, result[0].Price);
        _orderStoreMock.Verify(s => s.GetAllOrdersAsync(), Times.Once);
    }

    [Fact]
    public async Task GetOrdersAsync_ReturnsEmptyListWhenStoreHasNoOrders()
    {
        _orderStoreMock
            .Setup(s => s.GetAllOrdersAsync())
            .ReturnsAsync(new List<Order>());

        var result = await _sut.GetOrdersAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
        _orderStoreMock.Verify(s => s.GetAllOrdersAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_DelegatesToStore()
    {
        var order = new Order(1, "Product", 3, 14.99m);
        _orderStoreMock
            .Setup(s => s.CreateOrderAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        await _sut.CreateOrderAsync(order);

        _orderStoreMock.Verify(
            s => s.CreateOrderAsync(It.Is<Order>(o =>
                o.Id == 1 &&
                o.ProductName == "Product" &&
                o.Quantity == 3 &&
                o.Price == 14.99m)),
            Times.Once);
    }

    [Fact]
    public async Task CancelOrderAsync_ReturnsTrue_WhenStoreCancelsOrder()
    {
        const int orderId = 42;
        _orderStoreMock
            .Setup(s => s.CancelOrderAsync(orderId))
            .ReturnsAsync(true);

        var result = await _sut.CancelOrderAsync(orderId);

        Assert.True(result);
        _orderStoreMock.Verify(s => s.CancelOrderAsync(orderId), Times.Once);
    }

    [Fact]
    public async Task CancelOrderAsync_ReturnsFalse_WhenStoreFailsToCancel()
    {
        const int orderId = 99;
        _orderStoreMock
            .Setup(s => s.CancelOrderAsync(orderId))
            .ReturnsAsync(false);

        var result = await _sut.CancelOrderAsync(orderId);

        Assert.False(result);
        _orderStoreMock.Verify(s => s.CancelOrderAsync(orderId), Times.Once);
    }
}
