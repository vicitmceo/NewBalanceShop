using System.Net;
using System.Net.Http.Json;
using NewBalanceShop.Application.DTO;
using NewBalanceShop.Tests.Integration.Infrastructure;
using Xunit;

namespace NewBalanceShop.Tests.Integration.Controllers;

public class AuthControllerIntegrationTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory = new();
    private readonly HttpClient _client;

    public AuthControllerIntegrationTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private static RegisterDto MakeRegisterDto(string email = "integration@example.com") => new()
    {
        FullName = "Тест Тестенко",
        Email = email,
        Password = "Passw0rd!",
        City = "Kyiv",
        Country = "UA",
        Phone = "+380000000000"
    };

    [Fact]
    public async Task Register_WithNewEmail_ReturnsOk()
    {
        var response = await _client.PostAsJsonAsync("/api/Auth/register", MakeRegisterDto());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        await _client.PostAsJsonAsync("/api/Auth/register", MakeRegisterDto());

        var response = await _client.PostAsJsonAsync("/api/Auth/register", MakeRegisterDto());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOk()
    {
        await _client.PostAsJsonAsync("/api/Auth/register", MakeRegisterDto());
        await _client.PostAsync("/api/Auth/logout", null);

        var response = await _client.PostAsJsonAsync("/api/Auth/login",
            new LoginDto { Email = "integration@example.com", Password = "Passw0rd!" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        await _client.PostAsJsonAsync("/api/Auth/register", MakeRegisterDto());

        var response = await _client.PostAsJsonAsync("/api/Auth/login",
            new LoginDto { Email = "integration@example.com", Password = "wrong" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithoutLogin_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/Auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_AfterRegister_ReturnsCustomer()
    {
        await _client.PostAsJsonAsync("/api/Auth/register", MakeRegisterDto());

        var response = await _client.GetAsync("/api/Auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>();
        Assert.Equal("integration@example.com", customer!.Email);
    }

    [Fact]
    public async Task Logout_ClearsSession_MeReturnsUnauthorized()
    {
        await _client.PostAsJsonAsync("/api/Auth/register", MakeRegisterDto());

        await _client.PostAsync("/api/Auth/logout", null);
        var response = await _client.GetAsync("/api/Auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Google_WithoutFirebaseConfigured_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/Auth/google", new GoogleLoginDto { IdToken = "any" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
