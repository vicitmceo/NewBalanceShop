using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NewBalanceShop.Application.DTO;
using NewBalanceShop.Application.Interfaces;

namespace NewBalanceShop.Presentation.Controllers;

// Автентифікація на сесіях (ті самі AddSession/UseSession, що й для кошика,
// див. CartController): після логіну CustomerId кладеться в сесію-куку,
// окремий JWT/токен для навчального проєкту зайвий.
/// <summary>Реєстрація, вхід, вихід і поточний користувач (автентифікація на сесійній куці).</summary>
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
    /// <summary>Реєструє покупця й одразу входить.</summary>
    /// <param name="dto">Дані реєстрації.</param>
    /// <returns>200 і профіль, 400 якщо email зайнятий.</returns>
    [HttpPost("register")]
    [EnableRateLimiting("auth")]
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
    /// <summary>Вхід за email і паролем.</summary>
    /// <param name="dto">Облікові дані.</param>
    /// <returns>200 і профіль, 401 при невдачі.</returns>
    [HttpPost("login")]
    [EnableRateLimiting("auth")]
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

    // POST api/auth/google — вхід через кнопку "Увійти через Google" (Firebase Authentication)
    /// <summary>Вхід через Google за Firebase ID-токеном.</summary>
    /// <param name="dto">Токен.</param>
    /// <returns>200 і профіль, 401 при невдачі.</returns>
    [HttpPost("google")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<CustomerDto>> Google(GoogleLoginDto dto)
    {
        try
        {
            var customer = await _authService.GoogleLoginAsync(dto.IdToken);
            HttpContext.Session.SetInt32(CustomerIdSessionKey, customer.Id);
            return Ok(customer);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    // POST api/auth/logout
    /// <summary>Вихід: прибирає покупця із сесії.</summary>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove(CustomerIdSessionKey);
        return NoContent();
    }

    // GET api/auth/me — поточний залогінений покупець (null, якщо сесія не має логіну)
    /// <summary>Профіль поточного користувача.</summary>
    /// <returns>200 або 401, якщо вхід не виконано.</returns>
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
