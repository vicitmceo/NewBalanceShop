using System.Net;
using System.Net.Http.Json;
using NewBalanceShop.Application.DTO;
using NewBalanceShop.Tests.Integration.Infrastructure;
using Xunit;

namespace NewBalanceShop.Tests.Integration.Controllers;

public class CategoriesControllerIntegrationTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory = new();
    private readonly HttpClient _client;

    public CategoriesControllerIntegrationTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task GetAll_ReturnsSeededCategoriesOrderedById()
    {
        var response = await _client.GetAsync("/api/Categories");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
        Assert.Equal(new[] { 1, 2, 3 }, categories!.Select(c => c.Id));
        Assert.Equal("Кросівки", categories![0].Name);
    }

    [Fact]
    public async Task GetAll_CalledTwice_ReturnsSameResult()
    {
        var first = await _client.GetFromJsonAsync<List<CategoryDto>>("/api/Categories");
        var second = await _client.GetFromJsonAsync<List<CategoryDto>>("/api/Categories");

        Assert.Equal(first!.Select(c => c.Name), second!.Select(c => c.Name));
    }
}
