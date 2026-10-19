using Microsoft.Extensions.Caching.Memory;
using NewBalanceShop.Application.DTO;
using NewBalanceShop.Application.Interfaces;
using NewBalanceShop.Domain.Entities;
using NewBalanceShop.Domain.Interfaces;

namespace NewBalanceShop.Application.Services;

/// <summary>Реалізація <see cref="ICategoryService"/>: список категорій тримається в <see cref="IMemoryCache"/>, щоб не ходити в БД на кожен запит каталогу.</summary>
public class CategoryService : ICategoryService
{
    /// <summary>Ключ запису в кеші.</summary>
    public const string CacheKey = "categories:all";

    /// <summary>Час життя запису в кеші.</summary>
    public static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private readonly IRepository<Category> _categoryRepository;
    private readonly IMemoryCache _cache;

    public CategoryService(IRepository<Category> categoryRepository, IMemoryCache cache)
    {
        _categoryRepository = categoryRepository;
        _cache = cache;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        var cached = await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTtl;
            var categories = await _categoryRepository.GetAllAsync();
            return categories.OrderBy(c => c.Id).Select(c => new CategoryDto { Id = c.Id, Name = c.Name }).ToList();
        });

        // копія, щоб виклики не могли змінити спільний закешований список
        return cached!.Select(c => new CategoryDto { Id = c.Id, Name = c.Name }).ToList();
    }
}
