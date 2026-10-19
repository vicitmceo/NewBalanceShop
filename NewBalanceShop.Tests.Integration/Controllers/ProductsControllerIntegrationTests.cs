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

    private async Task<ProductDto> CreateProductAsync(string name, string gender, string type, decimal price = 1000m, decimal? oldPrice = null)
    {
        var response = await _client.PostAsJsonAsync("/api/Products", new ProductDto
        {
            Name = name, CategoryId = 1, Gender = gender, ProductType = type, Price = price, OldPrice = oldPrice, Stock = 5
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ProductDto>())!;
    }

    private async Task SeedCatalogAsync()
    {
        await LoginAsAdminAsync();
        await CreateProductAsync("Zeta Men Runner", "men", "Running", 2000m, 2500m);
        await CreateProductAsync("Zeta Women Runner", "women", "Running", 2100m);
        await CreateProductAsync("Zeta Kids Shoe", "kids", "Lifestyle", 900m, 1200m);
        await CreateProductAsync("Zeta Unisex Socks", "unisex", "Socks", 300m);
    }

    private async Task<List<string>> ZetaNamesAsync(string query)
    {
        var products = await _client.GetFromJsonAsync<List<ProductDto>>("/api/Products?q=Zeta&" + query);
        return products!.Select(p => p.Name).ToList();
    }

    [Fact]
    public async Task GetAll_SeededProductsAreUnisexNotOnSaleWithoutType()
    {
        var products = await _client.GetFromJsonAsync<List<ProductDto>>("/api/Products");

        Assert.All(products!, p =>
        {
            Assert.Equal("unisex", p.Gender);
            Assert.Equal(string.Empty, p.ProductType);
            Assert.Null(p.OldPrice);
        });
    }

    [Fact]
    public async Task Create_WithGenderTypeAndOldPrice_RoundTripsThroughApi()
    {
        await LoginAsAdminAsync();

        var created = await CreateProductAsync("Zeta Sale Jacket", "women", "Jackets & Vests", 1500m, 2000m);
        var fetched = await _client.GetFromJsonAsync<ProductDto>($"/api/Products/{created.Id}");

        Assert.Equal("women", fetched!.Gender);
        Assert.Equal("Jackets & Vests", fetched.ProductType);
        Assert.Equal(2000m, fetched.OldPrice);
    }

    [Fact]
    public async Task Create_WithoutGender_DefaultsToUnisex()
    {
        await LoginAsAdminAsync();

        var response = await _client.PostAsJsonAsync("/api/Products", new { name = "No gender", categoryId = 1, price = 10, stock = 1 });
        var created = await response.Content.ReadFromJsonAsync<ProductDto>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("unisex", created!.Gender);
        Assert.Null(created.OldPrice);
    }

    [Fact]
    public async Task Create_WithUnknownGender_ReturnsBadRequest()
    {
        await LoginAsAdminAsync();

        var response = await _client.PostAsJsonAsync("/api/Products",
            new ProductDto { Name = "X", CategoryId = 1, Price = 1m, Gender = "alien" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithOldPriceNotAbovePrice_ReturnsBadRequest()
    {
        await LoginAsAdminAsync();

        var response = await _client.PostAsJsonAsync("/api/Products",
            new ProductDto { Name = "X", CategoryId = 1, Price = 100m, OldPrice = 100m });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_ChangesGenderTypeAndClearsSale()
    {
        await LoginAsAdminAsync();
        var created = await CreateProductAsync("Zeta Update", "men", "Running", 1000m, 1500m);

        var put = await _client.PutAsJsonAsync($"/api/Products/{created.Id}",
            new ProductDto { Name = "Zeta Update", CategoryId = 1, Gender = "kids", ProductType = "Socks", Price = 1000m, OldPrice = null });
        var fetched = await _client.GetFromJsonAsync<ProductDto>($"/api/Products/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);
        Assert.Equal("kids", fetched!.Gender);
        Assert.Equal("Socks", fetched.ProductType);
        Assert.Null(fetched.OldPrice);
    }

    [Fact]
    public async Task GetAll_FilterByGender_MenAndWomenIncludeUnisexKidsOnlyKids()
    {
        await SeedCatalogAsync();

        Assert.Equal(new[] { "Zeta Men Runner", "Zeta Unisex Socks" }, await ZetaNamesAsync("gender=men"));
        Assert.Equal(new[] { "Zeta Women Runner", "Zeta Unisex Socks" }, await ZetaNamesAsync("gender=women"));
        Assert.Equal(new[] { "Zeta Kids Shoe" }, await ZetaNamesAsync("gender=kids"));
    }

    [Fact]
    public async Task GetAll_FilterByGender_ExcludesSeededUnisexSneakersFromKids()
    {
        await SeedCatalogAsync();

        var kids = await _client.GetFromJsonAsync<List<ProductDto>>("/api/Products?gender=kids");
        var men = await _client.GetFromJsonAsync<List<ProductDto>>("/api/Products?gender=men");

        Assert.Single(kids!);
        Assert.Equal(18 + 2, men!.Count);
    }

    [Fact]
    public async Task GetAll_FilterByType_IsCaseInsensitiveExactMatch()
    {
        await SeedCatalogAsync();

        Assert.Equal(new[] { "Zeta Men Runner", "Zeta Women Runner" }, await ZetaNamesAsync("type=running"));
        Assert.Equal(new[] { "Zeta Unisex Socks" }, await ZetaNamesAsync("type=SOCKS"));
        Assert.Empty(await ZetaNamesAsync("type=Run"));
    }

    [Fact]
    public async Task GetAll_FilterBySale_ReturnsOnlyProductsWithOldPrice()
    {
        await SeedCatalogAsync();

        var sale = await _client.GetFromJsonAsync<List<ProductDto>>("/api/Products?sale=true");

        Assert.Equal(new[] { "Zeta Men Runner", "Zeta Kids Shoe" }, sale!.Select(p => p.Name));
        Assert.All(sale!, p => Assert.NotNull(p.OldPrice));
    }

    [Fact]
    public async Task GetAll_CombinesGenderTypeSaleAndSort()
    {
        await SeedCatalogAsync();

        var result = await _client.GetFromJsonAsync<List<ProductDto>>("/api/Products?gender=men&type=Running&sale=true&sort=price_asc");

        Assert.Equal(new[] { "Zeta Men Runner" }, result!.Select(p => p.Name));
    }

    [Fact]
    public async Task GetAll_SearchMatchesProductTypeAndBrandCaseInsensitively()
    {
        await LoginAsAdminAsync();
        await CreateProductAsync("Plain Name", "men", "Jackets & Vests");

        var byType = await _client.GetFromJsonAsync<List<ProductDto>>("/api/Products?q=JACKETS");
        var byBrand = await _client.GetFromJsonAsync<List<ProductDto>>("/api/Products?q=new%20balance&categoryId=1&gender=men");

        Assert.Equal(new[] { "Plain Name" }, byType!.Select(p => p.Name));
        Assert.NotEmpty(byBrand!);
    }

    [Fact]
    public async Task GetAll_SortNew_ReturnsNewestFirst()
    {
        await SeedCatalogAsync();

        var result = await _client.GetFromJsonAsync<List<ProductDto>>("/api/Products?sort=new");

        Assert.Equal("Zeta Unisex Socks", result![0].Name);
        Assert.Equal(result.OrderByDescending(p => p.Id).Select(p => p.Id), result.Select(p => p.Id));
    }
}
