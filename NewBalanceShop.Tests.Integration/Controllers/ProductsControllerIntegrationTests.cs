using System.Net;
using System.Net.Http.Json;
using NewBalanceShop.Application.DTO;
using NewBalanceShop.Tests.Integration.Infrastructure;
using Xunit;

namespace NewBalanceShop.Tests.Integration.Controllers;

// Нова CustomWebApplicationFactory (і нова InMemory-БД) на кожен тест-метод,
// бо xUnit створює новий екземпляр класу під кожен [Fact] — тести повністю ізольовані.
public class ProductsControllerIntegrationTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory = new();
    private readonly HttpClient _client;

    public ProductsControllerIntegrationTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/Products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsSeededProducts()
    {
        var products = await _client.GetFromJsonAsync<List<ProductDto>>("/api/Products");

        Assert.NotNull(products);
        Assert.Equal(3, products!.Count);
    }

    [Fact]
    public async Task GetAll_WithCategoryId_ReturnsFilteredProducts()
    {
        var products = await _client.GetFromJsonAsync<List<ProductDto>>("/api/Products?categoryId=2");

        Assert.NotNull(products);
        Assert.All(products!, p => Assert.Equal(2, p.CategoryId));
    }

    [Fact]
    public async Task GetById_WithExistingId_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/Products/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/Products/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithValidProduct_ReturnsCreated()
    {
        var dto = new ProductDto { Name = "New Balance 990", CategoryId = 1, Price = 5999m, Stock = 4 };

        var response = await _client.PostAsJsonAsync("/api/Products", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithExistingProduct_ReturnsNoContent()
    {
        var dto = new ProductDto { Name = "Updated name", CategoryId = 1, Price = 100m, Stock = 1 };

        var response = await _client.PutAsJsonAsync("/api/Products/1", dto);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithNonExistingProduct_ReturnsNotFound()
    {
        var response = await _client.PutAsJsonAsync("/api/Products/9999", new ProductDto());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithExistingProduct_ReturnsNoContentAndRemovesIt()
    {
        var deleteResponse = await _client.DeleteAsync("/api/Products/1");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync("/api/Products/1");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_WithNonExistingProduct_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/Products/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
