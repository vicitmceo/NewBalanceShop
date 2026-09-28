using Moq;
using NewBalanceShop.Application.DTO;
using NewBalanceShop.Application.Services;
using NewBalanceShop.Domain.Entities;
using NewBalanceShop.Domain.Interfaces;
using Xunit;

namespace NewBalanceShop.Tests.Unit.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<ICustomerRepository> _customerRepo = new();
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _sut = new OrderService(_orderRepo.Object, _productRepo.Object, _customerRepo.Object);
    }

    private static Customer MakeCustomer(int id = 1) => new() { Id = id, FullName = "Тест", Email = "t@t.com", PasswordHash = "h" };

    private static Product MakeProduct(int id = 1, decimal price = 100m, int stock = 10) =>
        new() { Id = id, Name = "Товар", CategoryId = 1, Price = price, Stock = stock };

    private static CreateOrderDto MakeCreateDto(int customerId = 1, int productId = 1, int quantity = 2) => new()
    {
        CustomerId = customerId,
        Items = new List<CreateOrderItemDto> { new() { ProductId = productId, Quantity = quantity } }
    };

    [Fact]
    public async Task CreateAsync_WithNonExistingCustomer_ThrowsInvalidOperationException()
    {
        _customerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Customer?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(MakeCreateDto()));
    }

    [Fact]
    public async Task CreateAsync_WithNonExistingProduct_ThrowsInvalidOperationException()
    {
        _customerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeCustomer());
        _productRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(MakeCreateDto()));
    }

    [Fact]
    public async Task CreateAsync_WithInsufficientStock_ThrowsInvalidOperationException()
    {
        _customerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeCustomer());
        _productRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeProduct(stock: 1));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(MakeCreateDto(quantity: 5)));
        Assert.Contains("Недостатньо товару", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_DecreasesProductStock()
    {
        var product = MakeProduct(stock: 10);
        _customerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeCustomer());
        _productRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
        _orderRepo.Setup(r => r.GetByIdWithItemsAsync(It.IsAny<int>())).ReturnsAsync(new Order { Id = 1, CustomerId = 1 });

        await _sut.CreateAsync(MakeCreateDto(quantity: 3));

        Assert.Equal(7, product.Stock);
        _productRepo.Verify(r => r.Update(product), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_CalculatesTotalPriceCorrectly()
    {
        var product = MakeProduct(price: 150m, stock: 10);
        _customerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeCustomer());
        _productRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        Order? saved = null;
        _orderRepo.Setup(r => r.AddAsync(It.IsAny<Order>())).Callback<Order>(o => saved = o).Returns(Task.CompletedTask);
        _orderRepo.Setup(r => r.GetByIdWithItemsAsync(It.IsAny<int>())).ReturnsAsync(() => saved);

        var result = await _sut.CreateAsync(MakeCreateDto(quantity: 4));

        Assert.Equal(600m, result.TotalPrice);
    }

    [Fact]
    public async Task CreateAsync_WithValidOrder_SavesAndReturnsDto()
    {
        var product = MakeProduct();
        _customerRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeCustomer());
        _productRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        Order? saved = null;
        _orderRepo.Setup(r => r.AddAsync(It.IsAny<Order>())).Callback<Order>(o => saved = o).Returns(Task.CompletedTask);
        _orderRepo.Setup(r => r.GetByIdWithItemsAsync(It.IsAny<int>())).ReturnsAsync(() => saved);

        var result = await _sut.CreateAsync(MakeCreateDto());

        Assert.Equal(1, result.CustomerId);
        _orderRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsOrder()
    {
        _orderRepo.Setup(r => r.GetByIdWithItemsAsync(5)).ReturnsAsync(new Order { Id = 5, CustomerId = 1 });

        var result = await _sut.GetByIdAsync(5);

        Assert.NotNull(result);
        Assert.Equal(5, result!.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
    {
        _orderRepo.Setup(r => r.GetByIdWithItemsAsync(999)).ReturnsAsync((Order?)null);

        var result = await _sut.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByCustomerAsync_ReturnsCustomerOrders()
    {
        _orderRepo.Setup(r => r.GetByCustomerAsync(1)).ReturnsAsync(new List<Order> { new() { Id = 1, CustomerId = 1 }, new() { Id = 2, CustomerId = 1 } });

        var result = await _sut.GetByCustomerAsync(1);

        Assert.Equal(2, result.Count);
    }
}
