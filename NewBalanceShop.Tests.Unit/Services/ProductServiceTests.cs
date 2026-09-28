using Moq;
using NewBalanceShop.Application.DTO;
using NewBalanceShop.Application.Services;
using NewBalanceShop.Domain.Entities;
using NewBalanceShop.Domain.Interfaces;
using Xunit;

namespace NewBalanceShop.Tests.Unit.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repo = new();
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _sut = new ProductService(_repo.Object);
    }

    private static Product MakeProduct(int id = 1, int categoryId = 1) => new()
    {
        Id = id,
        Name = "New Balance 574",
        Brand = "New Balance",
        CategoryId = categoryId,
        Size = "42",
        Color = "Сірий",
        Price = 3299m,
        Stock = 15,
        ImageUrl = "/img.jpg",
        Description = "desc"
    };

    [Fact]
    public async Task GetAllAsync_ReturnsAllProducts()
    {
        _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product> { MakeProduct(1), MakeProduct(2) });

        var result = await _sut.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsProduct()
    {
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeProduct(1));

        var result = await _sut.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("New Balance 574", result!.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
    {
        _repo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        var result = await _sut.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByCategoryAsync_ReturnsProductsForCategory()
    {
        _repo.Setup(r => r.GetByCategoryAsync(2)).ReturnsAsync(new List<Product> { MakeProduct(3, 2) });

        var result = await _sut.GetByCategoryAsync(2);

        Assert.Single(result);
        Assert.Equal(2, result[0].CategoryId);
    }

    [Fact]
    public async Task CreateAsync_AddsAndReturnsProduct()
    {
        var dto = new ProductDto { Name = "New product", CategoryId = 1, Price = 100m, Stock = 5 };

        _repo.Setup(r => r.AddAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
        _repo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _sut.CreateAsync(dto);

        Assert.Equal("New product", result.Name);
        _repo.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ResetsIdToZeroBeforeInsert()
    {
        var dto = new ProductDto { Id = 999, Name = "X", CategoryId = 1 };
        Product? captured = null;
        _repo.Setup(r => r.AddAsync(It.IsAny<Product>())).Callback<Product>(p => captured = p).Returns(Task.CompletedTask);

        await _sut.CreateAsync(dto);

        Assert.NotNull(captured);
        Assert.Equal(0, captured!.Id);
    }

    [Fact]
    public async Task UpdateAsync_WithExistingProduct_UpdatesFieldsAndReturnsTrue()
    {
        var existing = MakeProduct(1);
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        var dto = new ProductDto { Name = "Updated", Brand = "NB", CategoryId = 2, Size = "44", Color = "Синій", Price = 500m, Stock = 3, ImageUrl = "u", Description = "d" };

        var result = await _sut.UpdateAsync(1, dto);

        Assert.True(result);
        Assert.Equal("Updated", existing.Name);
        Assert.Equal(500m, existing.Price);
        _repo.Verify(r => r.Update(existing), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingProduct_ReturnsFalse()
    {
        _repo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        var result = await _sut.UpdateAsync(999, new ProductDto());

        Assert.False(result);
        _repo.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingProduct_RemovesAndReturnsTrue()
    {
        var existing = MakeProduct(1);
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);

        var result = await _sut.DeleteAsync(1);

        Assert.True(result);
        _repo.Verify(r => r.Remove(existing), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingProduct_ReturnsFalse()
    {
        _repo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        var result = await _sut.DeleteAsync(999);

        Assert.False(result);
        _repo.Verify(r => r.Remove(It.IsAny<Product>()), Times.Never);
    }
}
