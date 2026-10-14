using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using NewBalanceShop.Presentation.Controllers;
using Xunit;

namespace NewBalanceShop.Tests.Unit.Controllers;

public class GithubAuthControllerTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactory = new();
    private readonly Microsoft.Extensions.Caching.Memory.MemoryCache _cache = new(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions());
    private readonly Mock<NewBalanceShop.Application.Interfaces.IAuthService> _authService = new();
    private readonly GithubAuthController _sut;

    public GithubAuthControllerTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GitHub:ClientId"] = "test-client-id",
                ["Frontend:Url"] = "https://frontend.example.com"
            })
            .Build();

        var httpContext = new DefaultHttpContext { Session = new TestSession() };
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("api.example.com");

        _sut = new GithubAuthController(config, _httpClientFactory.Object, _authService.Object, _cache)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }

    [Fact]
    public void Login_RedirectsToGitHubAuthorizeUrl()
    {
        var result = Assert.IsType<RedirectResult>(_sut.Login());

        Assert.StartsWith("https://github.com/login/oauth/authorize", result.Url);
        Assert.Contains("client_id=test-client-id", result.Url);
        Assert.Contains(Uri.EscapeDataString("https://api.example.com/api/auth/github/callback"), result.Url);
    }

    [Fact]
    public async Task Callback_WithMissingCode_RedirectsWithMissingCodeError()
    {
        var result = Assert.IsType<RedirectResult>(await _sut.Callback(string.Empty));

        Assert.Equal("https://frontend.example.com?githubError=missing_code", result.Url);
        _httpClientFactory.Verify(f => f.CreateClient(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Me_WithoutSession_ReturnsUnauthorized()
    {
        var result = _sut.Me();

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void Me_WithSession_ReturnsStoredUserJson()
    {
        _sut.ControllerContext.HttpContext.Session.SetString("githubUser", "{\"Login\":\"octocat\"}");

        var result = Assert.IsType<ContentResult>(_sut.Me());

        Assert.Equal("application/json", result.ContentType);
        Assert.Contains("octocat", result.Content);
    }

    [Fact]
    public void Logout_RemovesSessionKey()
    {
        _sut.ControllerContext.HttpContext.Session.SetString("githubUser", "{}");

        var result = _sut.Logout();

        Assert.IsType<NoContentResult>(result);
        Assert.Null(_sut.ControllerContext.HttpContext.Session.GetString("githubUser"));
    }

    [Fact]
    public void Login_WithAllowedReturnUrl_PassesItAsState()
    {
        var result = Assert.IsType<RedirectResult>(_sut.Login("https://newbalance.cc.cd/"));

        Assert.Contains("state=" + Uri.EscapeDataString("https://newbalance.cc.cd"), result.Url);
    }

    [Fact]
    public void Login_WithForeignReturnUrl_IgnoresIt()
    {
        var result = Assert.IsType<RedirectResult>(_sut.Login("https://evil.example"));

        Assert.DoesNotContain("state=", result.Url);
    }

    [Fact]
    public async Task Callback_WithMissingCode_ReturnsToAllowedOriginFromState()
    {
        var result = Assert.IsType<RedirectResult>(await _sut.Callback(string.Empty, "https://nb.rafail.xyz"));

        Assert.Equal("https://nb.rafail.xyz?githubError=missing_code", result.Url);
    }

    [Fact]
    public async Task Callback_WithForeignState_FallsBackToConfiguredFrontend()
    {
        var result = Assert.IsType<RedirectResult>(await _sut.Callback(string.Empty, "https://evil.example"));

        Assert.Equal("https://frontend.example.com?githubError=missing_code", result.Url);
    }

    [Fact]
    public async Task Exchange_WithUnknownCode_ReturnsUnauthorized()
    {
        var result = await _sut.Exchange(new NewBalanceShop.Application.DTO.GithubExchangeDto { Code = "nope" });

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task Exchange_WithValidCode_StartsSession_AndCodeWorksOnlyOnce()
    {
        var grantType = typeof(GithubAuthController).GetNestedType("GithubGrant", System.Reflection.BindingFlags.NonPublic)!;
        var grant = Activator.CreateInstance(grantType, 7, "{\"Login\":\"octocat\"}")!;
        _cache.Set("gh-grant:abc", grant, TimeSpan.FromMinutes(1));
        _authService.Setup(a => a.GetByIdAsync(7)).ReturnsAsync(new NewBalanceShop.Application.DTO.CustomerDto { Id = 7, Email = "o@x" });

        var first = await _sut.Exchange(new NewBalanceShop.Application.DTO.GithubExchangeDto { Code = "abc" });

        Assert.IsType<OkObjectResult>(first.Result);
        Assert.Equal(7, _sut.ControllerContext.HttpContext.Session.GetInt32("customerId"));
        Assert.Contains("octocat", _sut.ControllerContext.HttpContext.Session.GetString("githubUser"));

        var second = await _sut.Exchange(new NewBalanceShop.Application.DTO.GithubExchangeDto { Code = "abc" });
        Assert.IsType<UnauthorizedObjectResult>(second.Result);
    }

    [Fact]
    public async Task Exchange_ForBlockedCustomer_ReturnsUnauthorized()
    {
        var grantType = typeof(GithubAuthController).GetNestedType("GithubGrant", System.Reflection.BindingFlags.NonPublic)!;
        _cache.Set("gh-grant:blocked", Activator.CreateInstance(grantType, 8, "{}")!, TimeSpan.FromMinutes(1));
        _authService.Setup(a => a.GetByIdAsync(8)).ReturnsAsync(new NewBalanceShop.Application.DTO.CustomerDto { Id = 8, IsBlocked = true });

        var result = await _sut.Exchange(new NewBalanceShop.Application.DTO.GithubExchangeDto { Code = "blocked" });

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }
}
