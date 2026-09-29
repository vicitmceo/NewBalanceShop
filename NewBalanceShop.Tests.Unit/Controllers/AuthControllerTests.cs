using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NewBalanceShop.Application.DTO;
using NewBalanceShop.Application.Interfaces;
using NewBalanceShop.Presentation.Controllers;
using Xunit;

namespace NewBalanceShop.Tests.Unit.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _service = new();
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _sut = new AuthController(_service.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Session = new TestSession() }
            }
        };
    }

    private static CustomerDto MakeCustomer(int id = 1) => new() { Id = id, Email = "t@t.com", FullName = "Тест" };

    [Fact]
    public async Task Register_WithValidData_ReturnsOk()
    {
        _service.Setup(s => s.RegisterAsync(It.IsAny<RegisterDto>())).ReturnsAsync(MakeCustomer());

        var result = await _sut.Register(new RegisterDto { Email = "t@t.com", Password = "pw" });

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task Register_WithValidData_SetsCustomerIdInSession()
    {
        _service.Setup(s => s.RegisterAsync(It.IsAny<RegisterDto>())).ReturnsAsync(MakeCustomer(42));

        await _sut.Register(new RegisterDto { Email = "t@t.com", Password = "pw" });

        Assert.Equal(42, _sut.ControllerContext.HttpContext.Session.GetInt32("customerId"));
    }

    [Fact]
    public async Task Register_WhenServiceThrows_ReturnsBadRequest()
    {
        _service.Setup(s => s.RegisterAsync(It.IsAny<RegisterDto>()))
            .ThrowsAsync(new InvalidOperationException("вже зареєстрований"));

        var result = await _sut.Register(new RegisterDto());

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOk()
    {
        _service.Setup(s => s.LoginAsync(It.IsAny<LoginDto>())).ReturnsAsync(MakeCustomer());

        var result = await _sut.Login(new LoginDto { Email = "t@t.com", Password = "pw" });

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_WithValidCredentials_SetsCustomerIdInSession()
    {
        _service.Setup(s => s.LoginAsync(It.IsAny<LoginDto>())).ReturnsAsync(MakeCustomer(7));

        await _sut.Login(new LoginDto { Email = "t@t.com", Password = "pw" });

        Assert.Equal(7, _sut.ControllerContext.HttpContext.Session.GetInt32("customerId"));
    }

    [Fact]
    public async Task Login_WhenServiceThrows_ReturnsUnauthorized()
    {
        _service.Setup(s => s.LoginAsync(It.IsAny<LoginDto>()))
            .ThrowsAsync(new InvalidOperationException("Невірний email або пароль."));

        var result = await _sut.Login(new LoginDto());

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public void Logout_RemovesCustomerIdFromSession()
    {
        _sut.ControllerContext.HttpContext.Session.SetInt32("customerId", 1);

        var result = _sut.Logout();

        Assert.IsType<NoContentResult>(result);
        Assert.Null(_sut.ControllerContext.HttpContext.Session.GetInt32("customerId"));
    }

    [Fact]
    public async Task Me_WithoutSession_ReturnsUnauthorized()
    {
        var result = await _sut.Me();

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task Me_WithValidSession_ReturnsCustomer()
    {
        _sut.ControllerContext.HttpContext.Session.SetInt32("customerId", 1);
        _service.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(MakeCustomer());

        var result = await _sut.Me();

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task Me_WhenSessionCustomerWasDeleted_ReturnsUnauthorizedAndClearsSession()
    {
        _sut.ControllerContext.HttpContext.Session.SetInt32("customerId", 1);
        _service.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((CustomerDto?)null);

        var result = await _sut.Me();

        Assert.IsType<UnauthorizedResult>(result.Result);
        Assert.Null(_sut.ControllerContext.HttpContext.Session.GetInt32("customerId"));
    }

    [Fact]
    public async Task Google_WithValidIdToken_ReturnsOkAndSetsSession()
    {
        _service.Setup(s => s.GoogleLoginAsync("valid-token")).ReturnsAsync(MakeCustomer(3));

        var result = await _sut.Google(new GoogleLoginDto { IdToken = "valid-token" });

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(3, _sut.ControllerContext.HttpContext.Session.GetInt32("customerId"));
    }

    [Fact]
    public async Task Google_WhenServiceThrows_ReturnsUnauthorized()
    {
        _service.Setup(s => s.GoogleLoginAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Недійсний токен Google."));

        var result = await _sut.Google(new GoogleLoginDto { IdToken = "bad-token" });

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }
}
