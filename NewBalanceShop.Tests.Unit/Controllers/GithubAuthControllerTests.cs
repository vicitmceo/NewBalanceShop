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

        _sut = new GithubAuthController(config, _httpClientFactory.Object)
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
}
