using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using NewBalanceShop.Tests.Integration.Infrastructure;
using Xunit;

namespace NewBalanceShop.Tests.Integration.Controllers;

public class PreferencesControllerIntegrationTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory = new();
    private readonly HttpClient _client;

    public PreferencesControllerIntegrationTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private async Task<string> GetThemeAsync()
    {
        var json = await _client.GetFromJsonAsync<JsonElement>("/api/Preferences/theme");
        return json.GetProperty("theme").GetString()!;
    }

    [Fact]
    public async Task GetTheme_WithoutCookie_ReturnsLight()
    {
        Assert.Equal("light", await GetThemeAsync());
    }

    [Fact]
    public async Task SetTheme_Dark_IsRememberedInCookie()
    {
        var response = await _client.PostAsync("/api/Preferences/theme/dark", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("dark", await GetThemeAsync());
    }

    [Fact]
    public async Task SetTheme_UnknownValue_ReturnsBadRequest()
    {
        var response = await _client.PostAsync("/api/Preferences/theme/neon", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("light", await GetThemeAsync());
    }
}
