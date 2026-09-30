using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NewBalanceShop.Application.DTO;
using NewBalanceShop.Application.Interfaces;
using NewBalanceShop.Presentation.Controllers;
using Xunit;

namespace NewBalanceShop.Tests.Unit.Controllers;

public class CustomersControllerTests
{
    private readonly Mock<ICustomerService> _customerService = new();
    private readonly Mock<IOrderService> _orderService = new();
    private readonly CustomersController _sut;

    public CustomersControllerTests()
    {
        _sut = new CustomersController(_customerService.Object, _orderService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Session = new TestSession() }
            }
        };
    }

    private void SignInAs(int customerId, bool isAdmin)
    {
        _sut.ControllerContext.HttpContext.Session.SetInt32("customerId", customerId);
        _customerService.Setup(s => s.GetByIdAsync(customerId))
            .ReturnsAsync(new CustomerDto { Id = customerId, IsAdmin = isAdmin });
    }

    [Fact]
    public async Task GetAll_WithoutSession_ReturnsUnauthorized()
    {
        var result = await _sut.GetAll();

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task GetAll_AsRegularCustomer_ReturnsForbidden()
    {
        SignInAs(1, isAdmin: false);

        var result = await _sut.GetAll();

        var status = Assert.IsType<StatusCodeResult>(result.Result);
        Assert.Equal(403, status.StatusCode);
    }

    [Fact]
    public async Task GetAll_AsAdmin_ReturnsOk()
    {
        SignInAs(1, isAdmin: true);
        _customerService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<CustomerDto> { new(), new() });

        var result = await _sut.GetAll();

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetById_AsOwner_ReturnsOk()
    {
        SignInAs(5, isAdmin: false);
        _customerService.Setup(s => s.GetByIdAsync(5)).ReturnsAsync(new CustomerDto { Id = 5 });

        var result = await _sut.GetById(5);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetById_AsAnotherRegularCustomer_ReturnsForbidden()
    {
        SignInAs(5, isAdmin: false);

        var result = await _sut.GetById(9);

        var status = Assert.IsType<StatusCodeResult>(result.Result);
        Assert.Equal(403, status.StatusCode);
    }

    [Fact]
    public async Task GetById_AsAdmin_CanViewAnyProfile()
    {
        SignInAs(1, isAdmin: true);
        _customerService.Setup(s => s.GetByIdAsync(9)).ReturnsAsync(new CustomerDto { Id = 9 });

        var result = await _sut.GetById(9);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ReturnsNotFound()
    {
        SignInAs(1, isAdmin: true);
        _customerService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((CustomerDto?)null);

        var result = await _sut.GetById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Update_AsOwner_ReturnsOk()
    {
        SignInAs(5, isAdmin: false);
        _customerService.Setup(s => s.UpdateAsync(5, It.IsAny<UpdateCustomerDto>())).ReturnsAsync(new CustomerDto { Id = 5 });

        var result = await _sut.Update(5, new UpdateCustomerDto());

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task Update_AsAnotherRegularCustomer_ReturnsForbidden()
    {
        SignInAs(5, isAdmin: false);

        var result = await _sut.Update(9, new UpdateCustomerDto());

        var status = Assert.IsType<StatusCodeResult>(result.Result);
        Assert.Equal(403, status.StatusCode);
    }

    [Fact]
    public async Task Update_WhenServiceThrows_ReturnsBadRequest()
    {
        SignInAs(5, isAdmin: false);
        _customerService.Setup(s => s.UpdateAsync(5, It.IsAny<UpdateCustomerDto>()))
            .ThrowsAsync(new InvalidOperationException("email вже використовується"));

        var result = await _sut.Update(5, new UpdateCustomerDto());

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetOrders_AsOwner_ReturnsOk()
    {
        SignInAs(5, isAdmin: false);
        _orderService.Setup(s => s.GetByCustomerAsync(5)).ReturnsAsync(new List<OrderDto>());

        var result = await _sut.GetOrders(5);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetOrders_AsAnotherRegularCustomer_ReturnsForbidden()
    {
        SignInAs(5, isAdmin: false);

        var result = await _sut.GetOrders(9);

        var status = Assert.IsType<StatusCodeResult>(result.Result);
        Assert.Equal(403, status.StatusCode);
    }

    [Fact]
    public async Task SetBlocked_AsAdmin_ReturnsOk()
    {
        SignInAs(1, isAdmin: true);
        _customerService.Setup(s => s.SetBlockedAsync(9, true)).ReturnsAsync(new CustomerDto { Id = 9, IsBlocked = true });

        var result = await _sut.SetBlocked(9, new SetBlockedDto { Blocked = true });

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task SetBlocked_AsRegularCustomer_ReturnsForbidden()
    {
        SignInAs(1, isAdmin: false);

        var result = await _sut.SetBlocked(9, new SetBlockedDto { Blocked = true });

        var status = Assert.IsType<StatusCodeResult>(result.Result);
        Assert.Equal(403, status.StatusCode);
    }

    [Fact]
    public async Task SetBlocked_WithNonExistingCustomer_ReturnsNotFound()
    {
        SignInAs(1, isAdmin: true);
        _customerService.Setup(s => s.SetBlockedAsync(999, true)).ReturnsAsync((CustomerDto?)null);

        var result = await _sut.SetBlocked(999, new SetBlockedDto { Blocked = true });

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Delete_AsAdmin_ReturnsNoContent()
    {
        SignInAs(1, isAdmin: true);
        _customerService.Setup(s => s.DeleteAsync(9)).ReturnsAsync(true);

        var result = await _sut.Delete(9);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_AsRegularCustomer_ReturnsForbidden()
    {
        SignInAs(1, isAdmin: false);

        var result = await _sut.Delete(9);

        var status = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(403, status.StatusCode);
    }

    [Fact]
    public async Task Delete_WithNonExistingCustomer_ReturnsNotFound()
    {
        SignInAs(1, isAdmin: true);
        _customerService.Setup(s => s.DeleteAsync(999)).ReturnsAsync(false);

        var result = await _sut.Delete(999);

        Assert.IsType<NotFoundResult>(result);
    }
}
