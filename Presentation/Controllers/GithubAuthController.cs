using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace NewBalanceShop.Presentation.Controllers;

// Вхід через GitHub (OAuth App, authorization code flow):
// 1) фронтенд лінкує кнопку на GET api/auth/github/login
// 2) GitHub після згоди користувача повертає код на GET api/auth/github/callback
// 3) бекенд обмінює код на access_token і забирає профіль користувача GitHub,
//    кладе його в сесію (ту саму сесійну куку, що й кошик/логін) і редіректить назад на фронтенд
[ApiController]
[Route("api/auth/github")]
public class GithubAuthController : ControllerBase
{
    private const string SessionKey = "githubUser";

    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;

    public GithubAuthController(IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        var clientId = _config["GitHub:ClientId"];
        var callbackUrl = $"{Request.Scheme}://{Request.Host}/api/auth/github/callback";
        var authorizeUrl = "https://github.com/login/oauth/authorize" +
            $"?client_id={Uri.EscapeDataString(clientId ?? "")}" +
            $"&redirect_uri={Uri.EscapeDataString(callbackUrl)}" +
            "&scope=read:user";

        return Redirect(authorizeUrl);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code)
    {
        var frontendUrl = _config["Frontend:Url"] ?? "https://newbalanceshop-frontend.vercel.app";

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

        return Redirect($"{frontendUrl}?githubLogin=success");
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        var json = HttpContext.Session.GetString(SessionKey);
        if (json is null) return Unauthorized();

        return Content(json, "application/json");
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove(SessionKey);
        return NoContent();
    }

    private record GithubUserDto(string Login, string? Name, string? AvatarUrl);
}
