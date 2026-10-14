using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NewBalanceShop.Application.Interfaces;
using NewBalanceShop.Presentation.Security;

namespace NewBalanceShop.Presentation.Controllers;

// Вхід через GitHub (OAuth App, authorization code flow):
// 1) фронтенд лінкує кнопку на GET api/auth/github/login
// 2) GitHub після згоди користувача повертає код на GET api/auth/github/callback
// 3) бекенд обмінює код на access_token і забирає профіль користувача GitHub,
//    кладе його в сесію (ту саму сесійну куку, що й кошик/логін) і редіректить назад на фронтенд
/// <summary>Вхід через GitHub (OAuth, authorization code flow).</summary>
[ApiController]
[Route("api/auth/github")]
public class GithubAuthController : ControllerBase
{
    private const string SessionKey = "githubUser";

    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAuthService _authService;

    // сайти, на які дозволено повертати користувача після входу (збігаються з білим списком CORS)
    private static readonly string[] AllowedReturnOrigins =
    {
        "https://newbalanceshop.vercel.app",
        "https://newbalanceshop-frontend.vercel.app",
        "https://new-balance-shop.vercel.app",
        "https://newbalance.cc.cd",
        "https://nb.rafail.xyz",
        "http://localhost:5173",
        "http://localhost:5180"
    };

    public GithubAuthController(IConfiguration config, IHttpClientFactory httpClientFactory, IAuthService authService)
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
        _authService = authService;
    }

    // повертає origin із білого списку або null: так параметр state не перетворюється на open redirect
    private static string? SafeOrigin(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl)) return null;
        return AllowedReturnOrigins.FirstOrDefault(o => string.Equals(o, returnUrl.TrimEnd('/'), StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Перенаправляє на сторінку авторизації GitHub.</summary>
    /// <param name="returnUrl">Origin сайту, на який повернути користувача (лише з білого списку).</param>
    [HttpGet("login")]
    public IActionResult Login([FromQuery] string? returnUrl = null)
    {
        var clientId = _config["GitHub:ClientId"];
        var callbackUrl = $"{Request.Scheme}://{Request.Host}/api/auth/github/callback";
        var authorizeUrl = "https://github.com/login/oauth/authorize" +
            $"?client_id={Uri.EscapeDataString(clientId ?? "")}" +
            $"&redirect_uri={Uri.EscapeDataString(callbackUrl)}" +
            "&scope=read:user";
        var origin = SafeOrigin(returnUrl);
        if (origin is not null) authorizeUrl += $"&state={Uri.EscapeDataString(origin)}";

        return Redirect(authorizeUrl);
    }

    /// <summary>Обмінює код на токен, зберігає профіль GitHub у сесії й повертає на фронтенд.</summary>
    /// <param name="code">Одноразовий код від GitHub.</param>
    /// <param name="state">Origin сайту, з якого почався вхід (повторно перевіряється за білим списком).</param>
    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string? state = null)
    {
        var frontendUrl = SafeOrigin(state) ?? _config["Frontend:Url"] ?? "https://newbalanceshop-frontend.vercel.app";

        if (string.IsNullOrEmpty(code)) return Redirect($"{frontendUrl}?githubError=missing_code");

        var clientId = _config["GitHub:ClientId"];
        var clientSecret = _config["GitHub:ClientSecret"];
        var callbackUrl = $"{Request.Scheme}://{Request.Host}/api/auth/github/callback";

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // крок 1: обмінюємо code на access_token
        var tokenResponse = await client.PostAsync("https://github.com/login/oauth/access_token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = clientId ?? "",
                ["client_secret"] = clientSecret ?? "",
                ["code"] = code,
                ["redirect_uri"] = callbackUrl,
            }));

        if (!tokenResponse.IsSuccessStatusCode) return Redirect($"{frontendUrl}?githubError=token_exchange_failed");

        var tokenJson = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync());
        if (!tokenJson.RootElement.TryGetProperty("access_token", out var tokenProp))
            return Redirect($"{frontendUrl}?githubError=no_access_token");

        var accessToken = tokenProp.GetString();

        // крок 2: забираємо профіль користувача GitHub
        var userRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
        userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        userRequest.Headers.UserAgent.Add(new ProductInfoHeaderValue("NewBalanceShop", "1.0"));

        var userResponse = await client.SendAsync(userRequest);
        if (!userResponse.IsSuccessStatusCode) return Redirect($"{frontendUrl}?githubError=user_fetch_failed");

        var userJson = JsonDocument.Parse(await userResponse.Content.ReadAsStringAsync()).RootElement;

        var githubUser = new GithubUserDto(
            userJson.GetProperty("login").GetString() ?? "",
            userJson.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null,
            userJson.TryGetProperty("avatar_url", out var avatarProp) ? avatarProp.GetString() : null
        );

        HttpContext.Session.SetString(SessionKey, JsonSerializer.Serialize(githubUser));

        // GitHub-вхід створює (або знаходить) повноцінного покупця, щоб можна було оформлювати замовлення
        try
        {
            var customer = await _authService.GithubLoginAsync(githubUser.Login, githubUser.Name);
            HttpContext.Session.SetInt32(SessionAuth.CustomerIdSessionKey, customer.Id);
        }
        catch (InvalidOperationException)
        {
            HttpContext.Session.Remove(SessionKey);
            return Redirect($"{frontendUrl}?githubError=account_blocked");
        }

        return Redirect($"{frontendUrl}?githubLogin=success");
    }

    /// <summary>Профіль GitHub із сесії або 401.</summary>
    [HttpGet("me")]
    public IActionResult Me()
    {
        var json = HttpContext.Session.GetString(SessionKey);
        if (json is null) return Unauthorized();

        return Content(json, "application/json");
    }

    /// <summary>Прибирає профіль GitHub із сесії.</summary>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove(SessionKey);
        return NoContent();
    }

    private record GithubUserDto(string Login, string? Name, string? AvatarUrl);
}
