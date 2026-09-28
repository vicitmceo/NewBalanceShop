using Microsoft.AspNetCore.Mvc;
using Moq;
using NewBalanceShop.Application.DTO;
using NewBalanceShop.Application.Interfaces;
using NewBalanceShop.Presentation.Controllers;
using Xunit;

namespace NewBalanceShop.Tests.Unit.Controllers;

public class OrdersControllerTests
{
    private readonly Mock<IOrderService> _service = new();
    private readonly OrdersController _sut;

    public OrdersControllerTests()
    {
        _sut = new OrdersController(_service.Object);
    }

    [Fact]
    public async Task GetById_WithExistingId_ReturnsOk()
    {
        _service.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new OrderDto { Id = 1 });

        var result = await _sut.GetById(1);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ReturnsNotFound()
    {
        _service.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((OrderDto?)null);

        var result = await _sut.GetById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetByCustomer_ReturnsCustomerOrders()
    {
        _service.Setup(s => s.GetByCustomerAsync(1)).ReturnsAsync(new List<OrderDto> { new(), new() });

        var result = await _sut.GetByCustomer(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var orders = Assert.IsAssignableFrom<IEnumerable<OrderDto>>(ok.Value);
        Assert.Equal(2, orders.Count());
    }

    [Fact]
    public async Task Create_WithValidOrder_ReturnsCreatedAtAction()
    {
        _service.Setup(s => s.CreateAsync(It.IsAny<CreateOrderDto>())).ReturnsAsync(new OrderDto { Id = 1 });
        var dto = new CreateOrderDto { CustomerId = 1, Items = new List<CreateOrderItemDto> { new() { ProductId = 1, Quantity = 1 } } };

        var result = await _sut.Create(dto);

        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task Create_WhenServiceThrows_ReturnsBadRequest()
    {
        _service.Setup(s => s.CreateAsync(It.IsAny<CreateOrderDto>()))
            .ThrowsAsync(new InvalidOperationException("Товар не знайдено"));
        var dto = new CreateOrderDto { CustomerId = 1, Items = new List<CreateOrderItemDto> { new() { ProductId = 999, Quantity = 1 } } };

        var result = await _sut.Create(dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_WithInvalidModelState_ReturnsBadRequest()
    {
        _sut.ModelState.AddModelError("Items", "required");

        var result = await _sut.Create(new CreateOrderDto());

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
