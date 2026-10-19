using Microsoft.EntityFrameworkCore;
using NewBalanceShop.Infrastructure.Data;
using NewBalanceShop.Infrastructure.Repositories;
using Xunit;

namespace NewBalanceShop.Tests.Integration.Repositories;

// Репозиторії на реальному ShopDbContext (EF InMemory із сідом із ShopDbContext.OnModelCreating).
public class ProductRepositoryTests : IDisposable
{
    private readonly ShopDbContext _db;
    private readonly ProductRepository _sut;

    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ShopDbContext>()
            .UseInMemoryDatabase($"ProductRepositoryTests_{Guid.NewGuid()}")
            .Options;
        _db = new ShopDbContext(options);
        _db.Database.EnsureCreated();
        _sut = new ProductRepository(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task GetByIdsAsync_ReturnsOnlyRequestedProducts()
    {
        var result = await _sut.GetByIdsAsync(new[] { 1, 3, 9999 });

        Assert.Equal(new[] { 1, 3 }, result.Select(p => p.Id).OrderBy(i => i));
    }

    [Fact]
    public async Task GetByIdsAsync_IgnoresDuplicateIds()
    {
        var result = await _sut.GetByIdsAsync(new[] { 2, 2, 2 });

        Assert.Single(result);
    }

    [Fact]
    public async Task GetByIdsAsync_WithNoIds_ReturnsEmptyList()
    {
        Assert.Empty(await _sut.GetByIdsAsync(Array.Empty<int>()));
    }

    [Fact]
    public async Task GetByIdsAsync_TracksEntities_SoStockChangesAreSaved()
    {
        var product = (await _sut.GetByIdsAsync(new[] { 1 })).Single();
        var before = product.Stock;
        product.Stock -= 1;

        await _sut.SaveChangesAsync();

        Assert.Equal(before - 1, (await _sut.GetByIdNoTrackingAsync(1))!.Stock);
    }

    [Fact]
    public async Task GetByIdNoTrackingAsync_ReturnsProductWithoutTrackingIt()
    {
        var product = await _sut.GetByIdNoTrackingAsync(1);

        Assert.NotNull(product);
        Assert.Equal(EntityState.Detached, _db.Entry(product!).State);
    }

    [Fact]
    public async Task GetByIdNoTrackingAsync_WithUnknownId_ReturnsNull()
    {
        Assert.Null(await _sut.GetByIdNoTrackingAsync(9999));
    }

    [Fact]
    public async Task GetByIdAsync_StillTracksEntityForUpdates()
    {
        var product = await _sut.GetByIdAsync(1);

        Assert.Equal(EntityState.Unchanged, _db.Entry(product!).State);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsDetachedEntities()
    {
        var all = await _sut.GetAllAsync();

        Assert.All(all, p => Assert.Equal(EntityState.Detached, _db.Entry(p).State));
    }
}
