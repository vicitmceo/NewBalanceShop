using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NewBalanceShop.Application.DTO;
using NewBalanceShop.Application.Interfaces;
using NewBalanceShop.Domain.Entities;
using NewBalanceShop.Presentation.Controllers;
using Xunit;

namespace NewBalanceShop.Tests.Unit.Controllers;

public class OrdersControllerTests
{
    private readonly Mock<IOrderService> _service = new();
    private readonly Mock<ICustomerService> _customers = new();
    private readonly OrdersController _sut;

    public OrdersControllerTests()
    {
        _sut = new OrdersController(_service.Object, _customers.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Session = new TestSession() }
            }
        };
    }

    private void SignInAs(int customerId, bool isAdmin = false, bool blocked = false)
    {
        _sut.ControllerContext.HttpContext.Session.SetInt32("customerId", customerId);
        _customers.Setup(s => s.GetByIdAsync(customerId))
            .ReturnsAsync(new CustomerDto { Id = customerId, IsAdmin = isAdmin, IsBlocked = blocked });
    }

    [Fact]
    public async Task GetById_WithoutSession_ReturnsUnauthorized()
    {
        var result = await _sut.GetById(1);

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task GetById_OwnOrder_ReturnsOk()
    {
        SignInAs(5);
        _service.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new OrderDto { Id = 1, CustomerId = 5 });

        var result = await _sut.GetById(1);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetById_ForeignOrder_ReturnsNotFound()
    {
        SignInAs(5);
        _service.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new OrderDto { Id = 1, CustomerId = 6 });

        var result = await _sut.GetById(1);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetById_ForeignOrderAsAdmin_ReturnsOk()
    {
        SignInAs(2, isAdmin: true);
        _service.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new OrderDto { Id = 1, CustomerId = 6 });

        var result = await _sut.GetById(1);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ReturnsNotFound()
    {
        SignInAs(5);
        _service.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((OrderDto?)null);

        var result = await _sut.GetById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetByCustomer_WithoutSession_ReturnsUnauthorized()
    {
        var result = await _sut.GetByCustomer(null);

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task GetByCustomer_ReturnsOwnOrders()
    {
        SignInAs(5);
        _service.Setup(s => s.GetByCustomerAsync(5)).ReturnsAsync(new List<OrderDto> { new(), new() });

        var result = await _sut.GetByCustomer(null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(2, Assert.IsAssignableFrom<IEnumerable<OrderDto>>(ok.Value).Count());
    }

    [Fact]
    public async Task GetByCustomer_ForeignCustomerId_ReturnsForbidden()
    {
        SignInAs(5);

        var result = await _sut.GetByCustomer(6);

        Assert.Equal(403, Assert.IsType<StatusCodeResult>(result.Result).StatusCode);
        _service.Verify(s => s.GetByCustomerAsync(6), Times.Never);
    }

    [Fact]
    public async Task GetByCustomer_ForeignCustomerIdAsAdmin_ReturnsOk()
    {
        SignInAs(2, isAdmin: true);
        _service.Setup(s => s.GetByCustomerAsync(6)).ReturnsAsync(new List<OrderDto>());

        var result = await _sut.GetByCustomer(6);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetByCustomer_BlockedUser_ReturnsUnauthorized()
    {
        SignInAs(5, blocked: true);

        var result = await _sut.GetByCustomer(null);

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task Create_AsGuest_UsesGuestCustomer()
    {
        _service.Setup(s => s.CreateAsync(It.IsAny<CreateOrderDto>())).ReturnsAsync(new OrderDto { Id = 1 });
        var dto = new CreateOrderDto { CustomerId = 777, Items = new List<CreateOrderItemDto> { new() { ProductId = 1, Quantity = 1 } } };

        var result = await _sut.Create(dto);

        Assert.IsType<CreatedAtActionResult>(result.Result);
        _service.Verify(s => s.CreateAsync(It.Is<CreateOrderDto>(d => d.CustomerId == Customer.GuestId)), Times.Once);
    }

    [Fact]
    public async Task Create_WhenSignedIn_IgnoresCustomerIdFromBody()
    {
        SignInAs(5);
        _service.Setup(s => s.CreateAsync(It.IsAny<CreateOrderDto>())).ReturnsAsync(new OrderDto { Id = 1 });
        var dto = new CreateOrderDto { CustomerId = 777, Items = new List<CreateOrderItemDto> { new() { ProductId = 1, Quantity = 1 } } };

        await _sut.Create(dto);

        _service.Verify(s => s.CreateAsync(It.Is<CreateOrderDto>(d => d.CustomerId == 5)), Times.Once);
    }

    [Fact]
    public async Task Create_WhenServiceThrows_ReturnsBadRequest()
    {
        _service.Setup(s => s.CreateAsync(It.IsAny<CreateOrderDto>()))
            .ThrowsAsync(new InvalidOperationException("Товар не знайдено"));
        var dto = new CreateOrderDto { Items = new List<CreateOrderItemDto> { new() { ProductId = 999, Quantity = 1 } } };

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
