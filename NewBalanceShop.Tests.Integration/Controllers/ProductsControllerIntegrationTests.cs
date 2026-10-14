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

    private async Task LoginAsAdminAsync()
    {
        _factory.SetTestAdminPassword();
        var response = await _client.PostAsJsonAsync("/api/Auth/login",
            new LoginDto { Email = "admin@newbalanceshop.com", Password = CustomWebApplicationFactory.TestAdminPassword });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task LoginAsRegularUserAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/Auth/register", new RegisterDto
        {
            FullName = "Звичайний Покупець", Email = "regular@example.com", Password = "Passw0rd!"
        });
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Create_WithoutLogin_ReturnsUnauthorized()
    {
        var dto = new ProductDto { Name = "X", CategoryId = 1, Price = 1m, Stock = 1 };

        var response = await _client.PostAsJsonAsync("/api/Products", dto);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_AsRegularUser_ReturnsForbidden()
    {
        await LoginAsRegularUserAsync();
        var dto = new ProductDto { Name = "X", CategoryId = 1, Price = 1m, Stock = 1 };

        var response = await _client.PostAsJsonAsync("/api/Products", dto);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Update_AsRegularUser_ReturnsForbidden()
    {
        await LoginAsRegularUserAsync();

        var response = await _client.PutAsJsonAsync("/api/Products/1", new ProductDto { Name = "X", CategoryId = 1, Price = 1m, Stock = 1 });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithoutLogin_ReturnsUnauthorizedAndKeepsProduct()
    {
        var response = await _client.DeleteAsync("/api/Products/1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/api/Products/1")).StatusCode);
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
        Assert.Equal(18, products!.Count);
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
        await LoginAsAdminAsync();
        var dto = new ProductDto { Name = "New Balance 990", CategoryId = 1, Price = 5999m, Stock = 4 };

        var response = await _client.PostAsJsonAsync("/api/Products", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithExistingProduct_ReturnsNoContent()
    {
        await LoginAsAdminAsync();
        var dto = new ProductDto { Name = "Updated name", CategoryId = 1, Price = 100m, Stock = 1 };

        var response = await _client.PutAsJsonAsync("/api/Products/1", dto);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithNonExistingProduct_ReturnsNotFound()
    {
        await LoginAsAdminAsync();
        var response = await _client.PutAsJsonAsync("/api/Products/9999", new ProductDto());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithExistingProduct_ReturnsNoContentAndRemovesIt()
    {
        await LoginAsAdminAsync();
        var deleteResponse = await _client.DeleteAsync("/api/Products/1");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync("/api/Products/1");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_WithNonExistingProduct_ReturnsNotFound()
    {
        await LoginAsAdminAsync();
        var response = await _client.DeleteAsync("/api/Products/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsSeededCatalogWithPhotosForSneakers()
    {
        var products = await _client.GetFromJsonAsync<List<ProductDto>>("/api/Products");

        Assert.True(products!.Count >= 10);
        Assert.All(products.Where(p => p.CategoryId == 1), p => Assert.StartsWith("/images/", p.ImageUrl));
    }

    [Fact]
    public async Task GetAll_WithFiltersAndSort_ReturnsMatchingProducts()
    {
        var products = await _client.GetFromJsonAsync<List<ProductDto>>(
            "/api/Products?categoryId=1&minPrice=4000&maxPrice=6000&sort=price_desc");

        Assert.NotEmpty(products!);
        Assert.All(products, p => Assert.InRange(p.Price, 4000m, 6000m));
        Assert.Equal(products.OrderByDescending(p => p.Price).Select(p => p.Id), products.Select(p => p.Id));
    }

    [Fact]
    public async Task GetAll_WithSearch_FindsByName()
    {
        var products = await _client.GetFromJsonAsync<List<ProductDto>>("/api/Products?q=hoodie");

        Assert.Single(products!);
        Assert.Contains("Hoodie", products![0].Name);
    }

    [Fact]
    public async Task GetAll_FilterBySize_MatchesAnySizeInTheList()
    {
        var products = await _client.GetFromJsonAsync<List<ProductDto>>("/api/Products?size=46");

        Assert.NotEmpty(products!);
        Assert.All(products, p => Assert.Contains("46", p.Size.Split(',')));
    }

    [Fact]
    public async Task GetAll_SeededSneakersHaveSeveralSizes_AndFirstOneHasGallery()
    {
        var products = await _client.GetFromJsonAsync<List<ProductDto>>("/api/Products?categoryId=1");

        Assert.All(products!, p => Assert.True(p.Size.Split(',').Length >= 4));
        Assert.Contains(",", products!.Single(p => p.Id == 1).ExtraImages);
    }

    [Fact]
    public async Task GetAll_ClothingHasPhotoInGallery_WhileMainImageStaysIllustration()
    {
        var hoodie = await _client.GetFromJsonAsync<ProductDto>("/api/Products/3");

        Assert.Equal(string.Empty, hoodie!.ImageUrl);
        Assert.StartsWith("/images/", hoodie.ExtraImages);
    }
}
