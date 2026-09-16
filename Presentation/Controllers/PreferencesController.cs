using Microsoft.AspNetCore.Mvc;

namespace NewBalanceShop.Presentation.Controllers;

// Кукі тут логічні: кольорова тема сайту — це налаштування конкретного
// браузера відвідувача, а не дані, які треба зберігати в БД чи привʼязувати
// до облікового запису (якого в магазині поки що й немає).
[ApiController]
[Route("api/[controller]")]
public class PreferencesController : ControllerBase
{
    private const string ThemeCookie = "nb_theme";

    // GET api/preferences/theme
    [HttpGet("theme")]
    public IActionResult GetTheme()
    {
        var theme = Request.Cookies[ThemeCookie] ?? "light";
        return Ok(new { theme });
    }

    // POST api/preferences/theme/dark  (або /light)
    [HttpPost("theme/{value}")]
    public IActionResult SetTheme(string value)
    {
        if (value != "light" && value != "dark")
            return BadRequest(new { error = "Тема має бути 'light' або 'dark'" });

        Response.Cookies.Append(ThemeCookie, value, new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddDays(365),
            HttpOnly = false // тему читає і клієнтський JS, щоб одразу пофарбувати сторінку
        });

        return Ok(new { theme = value });
    }
}
