using Microsoft.AspNetCore.Mvc;
using NewBalanceShop.Application.DTO;
using NewBalanceShop.Application.Interfaces;

namespace NewBalanceShop.Presentation.Controllers;

// Автентифікація на сесіях (ті самі AddSession/UseSession, що й для кошика,
// див. CartController): після логіну CustomerId кладеться в сесію-куку,
// окремий JWT/токен для навчального проєкту зайвий.
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private const string CustomerIdSessionKey = "customerId";

    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST api/auth/register
    [HttpPost("register")]
    public async Task<ActionResult<CustomerDto>> Register(RegisterDto dto)
    {
        try
        {
            var customer = await _authService.RegisterAsync(dto);
            HttpContext.Session.SetInt32(CustomerIdSessionKey, customer.Id);
            return Ok(customer);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // POST api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<CustomerDto>> Login(LoginDto dto)
    {
        try
        {
            var customer = await _authService.LoginAsync(dto);
            HttpContext.Session.SetInt32(CustomerIdSessionKey, customer.Id);
            return Ok(customer);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    // POST api/auth/logout
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove(CustomerIdSessionKey);
        return NoContent();
    }

    // GET api/auth/me — поточний залогінений покупець (null, якщо сесія не має логіну)
    [HttpGet("me")]
    public async Task<ActionResult<CustomerDto>> Me()
    {
        var customerId = HttpContext.Session.GetInt32(CustomerIdSessionKey);
        if (customerId is null) return Unauthorized();

        var customer = await _authService.GetByIdAsync(customerId.Value);
        if (customer is null)
        {
            HttpContext.Session.Remove(CustomerIdSessionKey);
            return Unauthorized();
        }

        return Ok(customer);
    }
}
