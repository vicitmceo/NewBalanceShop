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

        var result = await _sut.GetAll(null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var products = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(ok.Value);
        Assert.Equal(2, products.Count());
    }

    [Fact]
    public async Task GetAll_WithCategoryId_ReturnsFilteredProducts()
    {
        _service.Setup(s => s.GetByCategoryAsync(2)).ReturnsAsync(new List<ProductDto> { new() { CategoryId = 2 } });

        var result = await _sut.GetAll(2);

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
}
