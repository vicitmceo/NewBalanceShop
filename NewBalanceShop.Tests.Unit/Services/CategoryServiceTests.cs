using Microsoft.Extensions.Caching.Memory;
using Moq;
using NewBalanceShop.Application.Services;
using NewBalanceShop.Domain.Entities;
using NewBalanceShop.Domain.Interfaces;
using Xunit;

namespace NewBalanceShop.Tests.Unit.Services;

public class CategoryServiceTests
{
    private readonly Mock<IRepository<Category>> _repo = new();
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());
    private readonly CategoryService _sut;

    public CategoryServiceTests()
    {
        _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Category>
        {
            new() { Id = 2, Name = "Одяг" },
            new() { Id = 1, Name = "Кросівки" }
        });
        _sut = new CategoryService(_repo.Object, _cache);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsCategoriesOrderedById()
    {
        var result = await _sut.GetAllAsync();

        Assert.Equal(new[] { 1, 2 }, result.Select(c => c.Id));
        Assert.Equal("Кросівки", result[0].Name);
    }

    [Fact]
    public async Task GetAllAsync_CalledTwice_HitsRepositoryOnce()
    {
        await _sut.GetAllAsync();
        await _sut.GetAllAsync();

        _repo.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_StoresListInCache()
    {
        await _sut.GetAllAsync();

        Assert.True(_cache.TryGetValue(CategoryService.CacheKey, out _));
    }

    [Fact]
    public async Task GetAllAsync_AfterCacheEviction_ReloadsFromRepository()
    {
        await _sut.GetAllAsync();
        _cache.Remove(CategoryService.CacheKey);

        await _sut.GetAllAsync();

        _repo.Verify(r => r.GetAllAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsCopies_SoCallersCannotCorruptTheCache()
    {
        var first = await _sut.GetAllAsync();
        first[0].Name = "Змінено";
        first.Clear();

        var second = await _sut.GetAllAsync();

        Assert.Equal(2, second.Count);
        Assert.Equal("Кросівки", second[0].Name);
    }

    [Fact]
    public void CacheTtl_IsShort()
    {
        Assert.InRange(CategoryService.CacheTtl, TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(15));
    }
}
