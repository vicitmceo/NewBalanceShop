using Microsoft.AspNetCore.Mvc;
using Moq;
using NewBalanceShop.Application.DTO;
using NewBalanceShop.Application.Interfaces;
using NewBalanceShop.Presentation.Controllers;
using Xunit;

namespace NewBalanceShop.Tests.Unit.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _service = new();
    private readonly ProductsController _sut;

    public ProductsControllerTests()
    {
        _sut = new ProductsController(_service.Object);
    }

    [Fact]
    public async Task GetAll_WithoutCategoryId_ReturnsAllProducts()
    {
        _service.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<ProductDto> { new(), new() });

        var result = await _sut.GetAll(null, null, null, null, null, null, null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var products = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(ok.Value);
        Assert.Equal(2, products.Count());
    }

    [Fact]
    public async Task GetAll_WithCategoryId_ReturnsFilteredProducts()
    {
        _service.Setup(s => s.GetByCategoryAsync(2)).ReturnsAsync(new List<ProductDto> { new() { CategoryId = 2 } });

        var result = await _sut.GetAll(2, null, null, null, null, null, null);

        _service.Verify(s => s.GetByCategoryAsync(2), Times.Once);
        _service.Verify(s => s.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task GetById_WithExistingId_ReturnsOk()
    {
        _service.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(new ProductDto { Id = 1 });

        var result = await _sut.GetById(1);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ReturnsNotFound()
    {
        _service.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((ProductDto?)null);

        var result = await _sut.GetById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_WithValidProduct_ReturnsCreatedAtAction()
    {
        _service.Setup(s => s.CreateAsync(It.IsAny<ProductDto>())).ReturnsAsync(new ProductDto { Id = 1 });

        var result = await _sut.Create(new ProductDto { Name = "X", CategoryId = 1 });

        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task Update_WithExistingProduct_ReturnsNoContent()
    {
        _service.Setup(s => s.UpdateAsync(1, It.IsAny<ProductDto>())).ReturnsAsync(true);

        var result = await _sut.Update(1, new ProductDto());

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_WithNonExistingProduct_ReturnsNotFound()
    {
        _service.Setup(s => s.UpdateAsync(999, It.IsAny<ProductDto>())).ReturnsAsync(false);

        var result = await _sut.Update(999, new ProductDto());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_WithExistingProduct_ReturnsNoContent()
    {
        _service.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _sut.Delete(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_WithNonExistingProduct_ReturnsNotFound()
    {
        _service.Setup(s => s.DeleteAsync(999)).ReturnsAsync(false);

        var result = await _sut.Delete(999);

        Assert.IsType<NotFoundResult>(result);
    }

    private static List<ProductDto> Sample() => new()
    {
        new() { Id = 1, Name = "Alpha Runner", Brand = "New Balance", Gender = "men", ProductType = "Running", OldPrice = 3500m, Price = 3000m, Size = "42", Color = "Black", Description = "fast" },
        new() { Id = 2, Name = "Beta Hoodie", Brand = "New Balance", Gender = "women", ProductType = "Hoodies", Price = 1500m, Size = "L", Color = "Grey", Description = "warm" },
        new() { Id = 3, Name = "Gamma Cap", Brand = "Other", Gender = "unisex", ProductType = "Hats", OldPrice = 800m, Price = 500m, Size = "One size", Color = "Black", Description = "sun" },
    };

    private async Task<List<ProductDto>> Run(string? q = null, string? brand = null, decimal? min = null, decimal? max = null, string? size = null, string? sort = null,
        string? gender = null, string? type = null, bool? sale = null, List<ProductDto>? products = null)
    {
        _service.Setup(s => s.GetAllAsync()).ReturnsAsync(products ?? Sample());
        var result = await _sut.GetAll(null, q, brand, min, max, size, sort, gender, type, sale);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        return Assert.IsAssignableFrom<IEnumerable<ProductDto>>(ok.Value).ToList();
    }

    [Fact]
    public async Task GetAll_WithSearch_MatchesNameDescriptionOrColor()
    {
        Assert.Equal(new[] { 1 }, (await Run(q: "runner")).Select(p => p.Id));
        Assert.Equal(new[] { 2 }, (await Run(q: "WARM")).Select(p => p.Id));
        Assert.Equal(new[] { 1, 3 }, (await Run(q: "black")).Select(p => p.Id));
    }

    [Fact]
    public async Task GetAll_WithBrand_ReturnsOnlyThatBrand()
    {
        Assert.Equal(new[] { 3 }, (await Run(brand: "other")).Select(p => p.Id));
    }

    [Fact]
    public async Task GetAll_WithPriceRange_IsInclusive()
    {
        Assert.Equal(new[] { 2, 3 }, (await Run(min: 500m, max: 1500m)).Select(p => p.Id));
    }

    [Fact]
    public async Task GetAll_WithSize_ReturnsExactSize()
    {
        Assert.Equal(new[] { 2 }, (await Run(size: "l")).Select(p => p.Id));
    }

    [Theory]
    [InlineData("price_asc", new[] { 3, 2, 1 })]
    [InlineData("price_desc", new[] { 1, 2, 3 })]
    [InlineData("name", new[] { 1, 2, 3 })]
    public async Task GetAll_WithSort_OrdersProducts(string sort, int[] expectedIds)
    {
        Assert.Equal(expectedIds, (await Run(sort: sort)).Select(p => p.Id));
    }

    private static List<ProductDto> GenderSample() => new()
    {
        new() { Id = 10, Name = "Men Runner", Gender = "men", ProductType = "Running", Price = 100m },
        new() { Id = 11, Name = "Women Runner", Gender = "women", ProductType = "Running", Price = 100m },
        new() { Id = 12, Name = "Unisex Sneaker", Gender = "unisex", ProductType = "Lifestyle", Price = 100m },
        new() { Id = 13, Name = "Kids Runner", Gender = "kids", ProductType = "Running", Price = 100m },
        new() { Id = 14, Name = "Men Socks", Gender = "men", ProductType = "Socks", Price = 100m },
    };

    [Theory]
    [InlineData("men", new[] { 10, 12, 14 })]
    [InlineData("women", new[] { 11, 12 })]
    [InlineData("kids", new[] { 13 })]
    [InlineData("MEN", new[] { 10, 12, 14 })]
    [InlineData(" kids ", new[] { 13 })]
    [InlineData("unisex", new[] { 12 })]
    [InlineData("alien", new int[0])]
    public async Task GetAll_WithGender_MenAndWomenAlsoIncludeUnisex_KidsOnlyKids(string gender, int[] expectedIds)
    {
        var result = await Run(gender: gender, products: GenderSample());

        Assert.Equal(expectedIds, result.Select(p => p.Id));
    }

    [Fact]
    public async Task GetAll_WithoutGender_ReturnsEveryGender()
    {
        var result = await Run(products: GenderSample());

        Assert.Equal(5, result.Count);
    }

    [Theory]
    [InlineData("Running", new[] { 10, 11, 13 })]
    [InlineData("running", new[] { 10, 11, 13 })]
    [InlineData(" SOCKS ", new[] { 14 })]
    [InlineData("Run", new int[0])]
    public async Task GetAll_WithType_IsExactCaseInsensitiveMatch(string type, int[] expectedIds)
    {
        var result = await Run(type: type, products: GenderSample());

        Assert.Equal(expectedIds, result.Select(p => p.Id));
    }

    [Fact]
    public async Task GetAll_WithGenderAndType_CombinesFilters()
    {
        var result = await Run(gender: "men", type: "Running", products: GenderSample());

        Assert.Equal(new[] { 10 }, result.Select(p => p.Id));
    }

    [Fact]
    public async Task GetAll_WithSaleTrue_ReturnsOnlyProductsWithOldPrice()
    {
        Assert.Equal(new[] { 1, 3 }, (await Run(sale: true)).Select(p => p.Id));
    }

    [Fact]
    public async Task GetAll_WithSaleFalseOrMissing_DoesNotFilterBySale()
    {
        Assert.Equal(3, (await Run(sale: false)).Count);
        Assert.Equal(3, (await Run(sale: null)).Count);
    }

    [Fact]
    public async Task GetAll_WithSearch_MatchesBrandAndProductType()
    {
        Assert.Equal(new[] { 3 }, (await Run(q: "OTHER")).Select(p => p.Id));
        Assert.Equal(new[] { 1 }, (await Run(q: "running")).Select(p => p.Id));
        Assert.Equal(new[] { 3 }, (await Run(q: "hats")).Select(p => p.Id));
    }

    [Fact]
    public async Task GetAll_WithSortNew_ReturnsNewestFirst()
    {
        Assert.Equal(new[] { 3, 2, 1 }, (await Run(sort: "new")).Select(p => p.Id));
    }
}
